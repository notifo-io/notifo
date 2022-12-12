// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Notifo.Domain.Apps;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Scheduling;
using ISmsTemplateStore = Notifo.Domain.ChannelTemplates.IChannelTemplateStore<Notifo.Domain.Channels.Sms.SmsTemplate>;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.Sms.SmsJob>;

namespace Notifo.Domain.Channels.Sms;

public sealed class SmsChannel : ICommunicationChannel, IScheduleHandler<SmsJob>, ISmsCallback
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

    public SmsChannel(ILogger<SmsChannel> log, ILogStore logStore,
        IAppStore appStore,
        IIntegrationManager integrationManager,
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

    public IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelSetting settings, SendContext context)
    {
        if (!integrationManager.IsConfigured<ISmsSender>(context.App, notification))
        {
            yield break;
        }

        if (!string.IsNullOrWhiteSpace(context.User.PhoneNumber))
        {
            yield return new SendConfiguration
            {
                [PhoneNumber] = context.User.PhoneNumber
            };
        }
    }

    public async Task HandleCallbackAsync(ISmsSender sender, SmsCallbackResponse response,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("SmsChannel/HandleCallbackAsync"))
        {
            var (notificationId, to, result, details) = response;
            var notification = await userNotificationStore.FindAsync(notificationId, ct);

            if (notification != null)
            {
                await UpdateAsync(notification, result, sender, details);
            }

            userNotificationQueue.Complete(SmsJob.ComputeScheduleKey(notificationId, to));
        }
    }

    private async Task UpdateAsync(UserNotification notification, SmsResult result, ISmsSender sender, string? details)
    {
        if (!notification.Channels.TryGetValue(Name, out var channel))
        {
            // There is no activity on this channel.
            return;
        }

        // We create only one configuration for this channel. Therefore it must be the first.
        var (configurationId, status) = channel.Status.First();

        if (status.Status == ProcessStatus.Attempt)
        {
            var identifier = TrackingKey.ForNotification(notification, Name, configurationId);

            if (result == SmsResult.Delivered)
            {
                await userNotificationStore.TrackAsync(identifier, ProcessStatus.Handled);
            }
            else if (result == SmsResult.Failed)
            {
                var message = LogMessage.Sms_CallbackError(sender.Name, details);

                // Also log the error to the app log.
                await logStore.LogAsync(notification.AppId, message);

                await userNotificationStore.TrackAsync(identifier, ProcessStatus.Failed, message.Reason);
            }
        }
    }

    public async Task SendAsync(UserNotification notification, ChannelSetting setting, Guid configurationId, SendConfiguration properties, SendContext context,
        CancellationToken ct)
    {
        if (context.IsUpdate)
        {
            return;
        }

        if (!properties.TryGetValue(PhoneNumber, out var phoneNumber))
        {
            // Old configuration without a phone number.
            return;
        }

        using (Telemetry.Activities.StartActivity("SmsChannel/SendAsync"))
        {
            var job = new SmsJob(notification, setting, configurationId, phoneNumber);

            await userNotificationQueue.ScheduleAsync(
                job.ScheduleKey,
                job,
                job.Delay,
                false, ct);
        }
    }

    public Task HandleExceptionAsync(SmsJob job, Exception ex)
    {
        return UpdateAsync(job, ProcessStatus.Failed);
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
                await UpdateAsync(job, ProcessStatus.Skipped);
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
            var app = await appStore.GetCachedAsync(job.Tracking.AppId!, ct);

            if (app == null)
            {
                log.LogWarning("Cannot send email: App not found.");

                await UpdateAsync(job, ProcessStatus.Handled);
                return;
            }

            try
            {
                await UpdateAsync(job, ProcessStatus.Attempt);

                var senders = integrationManager.Resolve<ISmsSender>(app, job).Select(x => x.Target).ToList();

                if (senders.Count == 0)
                {
                    await SkipAsync(job, LogMessage.Integration_Removed(Name));
                    return;
                }

                await SendCoreAsync(app, job, senders, ct);
            }
            catch (DomainException ex)
            {
                await logStore.LogAsync(job.Tracking.AppId!, LogMessage.General_Exception(Name, ex));
                throw;
            }
        }
    }

    private async Task SendCoreAsync(App app, SmsJob job, List<ISmsSender> senders,
        CancellationToken ct)
    {
        var lastSender = senders[^1];

        foreach (var sender in senders)
        {
            try
            {
                var (skip, template) = await GetTemplateAsync(
                    job.Tracking.AppId!,
                    job.TemplateLanguage,
                    job.TemplateName,
                    ct);

                if (skip != null)
                {
                    await SkipAsync(job, skip.Value);
                }

                var text = smsFormatter.Format(template, job.PhoneText);

                var result = await sender.SendAsync(app, job.PhoneNumber, text, job.Tracking.UserNotificationId.ToString(), ct);

                // Some integrations provide the actual result via webhook at a later point.
                if (result == SmsResult.Delivered)
                {
                    await UpdateAsync(job, ProcessStatus.Handled);
                    return;
                }

                // If the SMS has been sent, but not delivered yet, we also do not try other integrations.
                if (result == SmsResult.Sent)
                {
                    return;
                }
            }
            catch (DomainException ex)
            {
                await logStore.LogAsync(job.Tracking.AppId!, LogMessage.General_Exception(sender.Name, ex));

                if (sender == lastSender)
                {
                    throw;
                }
            }
            catch (Exception)
            {
                if (sender == lastSender)
                {
                    throw;
                }
            }
        }
    }

    private Task UpdateAsync(SmsJob job, ProcessStatus status, string? reason = null)
    {
        return userNotificationStore.TrackAsync(job.Tracking, status, reason);
    }

    private async Task SkipAsync(SmsJob job, LogMessage message)
    {
        await logStore.LogAsync(job.Tracking.AppId!, message);

        await UpdateAsync(job, ProcessStatus.Skipped, message.Reason);
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

                    await logStore.LogAsync(appId, message);
                    break;
                }

            case TemplateResolveStatus.NotFound:
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
