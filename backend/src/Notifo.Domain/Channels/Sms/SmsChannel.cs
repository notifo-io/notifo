// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Scheduling;
using ISmsTemplateStore = Notifo.Domain.ChannelTemplates.IChannelTemplateStore<Notifo.Domain.Channels.Sms.SmsTemplate>;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.Sms.SmsJob>;

namespace Notifo.Domain.Channels.Sms;

public sealed class SmsChannel : ICommunicationChannel, IScheduleHandler<SmsJob>, ICallback<ISmsSender>
{
    private const string PhoneNumber = nameof(PhoneNumber);
    private readonly IAppStore appStore;
    private readonly IIntegrationManager integrationManager;
    private readonly ILogger<SmsChannel> log;
    private readonly ILogStore logStore;
    private readonly ISmsFormatter smsFormatter;
    private readonly ISmsTemplateStore smsTemplateStore;
    private readonly IUserNotificationQueue userNotificationQueue;
    private readonly IUserNotificationStore userNotificationStore;

    public string Name => Providers.Sms;

    public SmsChannel(
        IAppStore appStore,
        IIntegrationManager integrationManager,
        ILogger<SmsChannel> log,
        ILogStore logStore,
        ISmsFormatter smsFormatter,
        ISmsTemplateStore smsTemplateStore,
        IUserNotificationQueue userNotificationQueue,
        IUserNotificationStore userNotificationStore)
    {
        this.appStore = appStore;
        this.integrationManager = integrationManager;
        this.log = log;
        this.logStore = logStore;
        this.smsFormatter = smsFormatter;
        this.smsTemplateStore = smsTemplateStore;
        this.userNotificationQueue = userNotificationQueue;
        this.userNotificationStore = userNotificationStore;
    }

    public IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelContext context)
    {
        if (notification.Silent || string.IsNullOrWhiteSpace(context.User.PhoneNumber))
        {
            yield break;
        }

        if (!integrationManager.HasIntegration<ISmsSender>(context.App))
        {
            yield break;
        }

        yield return new SendConfiguration
        {
            [PhoneNumber] = context.User.PhoneNumber
        };
    }

    public async Task UpdateStatusAsync(ISmsSender source, string trackingToken, DeliveryResult result)
    {
        using (Telemetry.Activities.StartActivity("SmsChannel/HandleCallbackAsync"))
        {
            if (result == default)
            {
                return;
            }

            var token = TrackingToken.Parse(trackingToken);

            if (token.UserNotificationId == default)
            {
                return;
            }

            userNotificationQueue.Complete(MessagingJob.ComputeScheduleKey(token.UserNotificationId));

            var notification = await userNotificationStore.FindAsync(token.UserNotificationId);

            if (notification == null)
            {
                return;
            }

            var trackingKey = TrackingKey.ForNotification(notification, Name, token.ConfigurationId);

            await userNotificationStore.TrackAsync(trackingKey, result);

            if (!string.IsNullOrWhiteSpace(result.Detail))
            {
                var message = LogMessage.Sms_CallbackError(source.Definition.Type, result.Detail);

                // Also log the error to the app log.
                await logStore.LogAsync(notification.AppId, message);
            }
        }
    }

    public async Task SendAsync(UserNotification notification, ChannelContext context,
        CancellationToken ct)
    {
        if (context.IsUpdate)
        {
            return;
        }

        if (!context.Configuration.TryGetValue(PhoneNumber, out var phoneNumber))
        {
            // Old configuration without a phone number.
            return;
        }

        using (Telemetry.Activities.StartActivity("SmsChannel/SendAsync"))
        {
            var job = new SmsJob(notification, context, phoneNumber);

            await userNotificationQueue.ScheduleAsync(
                job.ScheduleKey,
                job,
                job.SendDelay,
                false, ct);
        }
    }

    public Task HandleExceptionAsync(SmsJob job, Exception ex)
    {
        return UpdateAsync(job, DeliveryResult.Failed());
    }

    public async Task<bool> HandleAsync(SmsJob job, bool isLastAttempt,
        CancellationToken ct)
    {
        var activityLinks = job.Links();
        var activityContext = Activity.Current?.Context ?? default;

        using (Telemetry.Activities.StartActivity("SmsChannel/HandleAsync", ActivityKind.Internal, activityContext, links: activityLinks))
        {
            if (await userNotificationStore.IsHandledAsync(job, this, ct))
            {
                await UpdateAsync(job, DeliveryResult.Skipped());
            }
            else
            {
                await SendJobAsync(job, ct);
            }

            return false;
        }
    }

    private async Task SendJobAsync(SmsJob job,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("Send"))
        {
            var app = await appStore.GetCachedAsync(job.Notification.AppId, ct);

            if (app == null)
            {
                log.LogWarning("Cannot send email: App not found.");

                await UpdateAsync(job, DeliveryResult.Handled);
                return;
            }

            try
            {
                await UpdateAsync(job, DeliveryResult.Attempt);

                var integrations = integrationManager.Resolve<ISmsSender>(app, job.Notification).ToList();

                if (integrations.Count == 0)
                {
                    await SkipAsync(job, LogMessage.Integration_Removed(Name));
                    return;
                }

                var (skip, message) = await BuildMessageAsync(job, ct);

                if (skip != null)
                {
                    await SkipAsync(job, skip.Value);
                    return;
                }

                var result = await SendCoreAsync(job, message!, integrations, ct);

                if (result.Status > DeliveryStatus.Attempt)
                {
                    await UpdateAsync(job, result);
                }
            }
            catch (DomainException ex)
            {
                await logStore.LogAsync(job.Notification.AppId, LogMessage.General_Exception(Name, ex));
                throw;
            }
        }
    }

    private async Task<DeliveryResult> SendCoreAsync(SmsJob job, SmsMessage message, List<ResolvedIntegration<ISmsSender>> integrations,
        CancellationToken ct)
    {
        var lastResult = default(DeliveryResult);

        foreach (var (_, context, sender) in integrations)
        {
            try
            {
                lastResult = await sender.SendAsync(context, message, ct);

                // We only sent notifications over the first successful integration.
                if (lastResult.Status >= DeliveryStatus.Sent)
                {
                    break;
                }
            }
            catch (DomainException ex)
            {
                await logStore.LogAsync(job.Notification.AppId, LogMessage.General_Exception(sender.Definition.Type, ex));

                // We only expose details of domain exceptions.
                lastResult = DeliveryResult.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                await logStore.LogAsync(job.Notification.AppId, LogMessage.General_InternalException(sender.Definition.Type, ex));

                if (sender == integrations[^1].System)
                {
                    throw;
                }

                lastResult = DeliveryResult.Failed();
            }
        }

        return lastResult;
    }

    private async Task SkipAsync(SmsJob job, LogMessage message)
    {
        await logStore.LogAsync(job.Notification.AppId, message);

        await UpdateAsync(job, DeliveryResult.Skipped(message.Reason));
    }

    private Task UpdateAsync(SmsJob job, DeliveryResult result)
    {
        return userNotificationStore.TrackAsync(job.AsTrackingKey(Name), result);
    }

    private async Task<(LogMessage? Skip, SmsMessage?)> BuildMessageAsync(SmsJob job,
        CancellationToken ct)
    {
        var (skip, template) = await GetTemplateAsync(
            job.Notification.AppId,
            job.Notification.UserLanguage,
            job.Template,
            ct);

        if (skip != default)
        {
            return (skip, null);
        }

        var message = new SmsMessage
        {
            To = job.PhoneNumber,
            // We might also format the text without the template if no primary template is defined.
            Text = smsFormatter.Format(template, job.Notification.Formatting.Subject)
        };

        return (default, message.Enrich(job, Name));
    }

    private async Task<(LogMessage? Skip, SmsTemplate?)> GetTemplateAsync(
        string appId,
        string language,
        string? name,
        CancellationToken ct)
    {
        var (status, template) = await smsTemplateStore.GetBestAsync(appId, name, language, ct);

        switch (status)
        {
            case TemplateResolveStatus.ResolvedWithFallback:
                {
                    var message = LogMessage.ChannelTemplate_ResolvedWithFallback(Name, name);

                    // We just log a warning here, but use the fallback template.
                    await logStore.LogAsync(appId, message);
                    break;
                }

            case TemplateResolveStatus.NotFound when string.IsNullOrWhiteSpace(name):
                {
                    var message = LogMessage.ChannelTemplate_ResolvedWithFallback(Name, name);

                    // If no name was specified we just accept that the template does not exist.
                    await logStore.LogAsync(appId, message);
                    break;
                }

            case TemplateResolveStatus.NotFound when !string.IsNullOrWhiteSpace(name):
                {
                    var message = LogMessage.ChannelTemplate_NotFound(Name, name);

                    return (message, null);
                }

            case TemplateResolveStatus.LanguageNotFound:
                {
                    var message = LogMessage.ChannelTemplate_LanguageNotFound(Name, language, name);

                    return (message, null);
                }
        }

        return (null, template);
    }
}
