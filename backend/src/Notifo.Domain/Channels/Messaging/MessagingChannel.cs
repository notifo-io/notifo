// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Scheduling;
using IMessagingTemplateStore = Notifo.Domain.ChannelTemplates.IChannelTemplateStore<Notifo.Domain.Channels.Messaging.MessagingTemplate>;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.Messaging.MessagingJob>;

namespace Notifo.Domain.Channels.Messaging;

public sealed class MessagingChannel : ICommunicationChannel, IScheduleHandler<MessagingJob>, ICallback<IMessagingSender>
{
    private const string IntegrationIds = nameof(IntegrationIds);
    private readonly IAppStore appStore;
    private readonly IIntegrationManager integrationManager;
    private readonly ILogger<MessagingChannel> log;
    private readonly ILogStore logStore;
    private readonly IMessagingFormatter messagingFormatter;
    private readonly IMessagingTemplateStore messagingTemplateStore;
    private readonly IUserNotificationQueue userNotificationQueue;
    private readonly IUserNotificationStore userNotificationStore;
    private readonly IUserStore userStore;

    public string Name => Providers.Messaging;

    public MessagingChannel(
        IAppStore appStore,
        IIntegrationManager integrationManager,
        ILogger<MessagingChannel> log,
        ILogStore logStore,
        IMessagingFormatter messagingFormatter,
        IMessagingTemplateStore messagingTemplateStore,
        IUserNotificationQueue userNotificationQueue,
        IUserNotificationStore userNotificationStore,
        IUserStore userStore)
    {
        this.appStore = appStore;
        this.log = log;
        this.logStore = logStore;
        this.integrationManager = integrationManager;
        this.messagingFormatter = messagingFormatter;
        this.messagingTemplateStore = messagingTemplateStore;
        this.userNotificationQueue = userNotificationQueue;
        this.userNotificationStore = userNotificationStore;
        this.userStore = userStore;
    }

    public IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelContext context)
    {
        if (notification.Silent)
        {
            yield break;
        }

        // Do not pass in the user notification so what we enable all integrations.
        var integrations = integrationManager.Resolve<IMessagingSender>(context.App, null).ToList();

        if (integrations.Count == 0)
        {
            yield break;
        }

        var configuration = new SendConfiguration();

        // Create a context to not expose the user details to the provider.
        var userContext = context.User.ToContext();

        foreach (var (_, _, sender) in integrations)
        {
            sender.AddTargets(configuration, userContext);
        }

        if (configuration.Count == 0)
        {
            yield break;
        }

        yield return configuration;
    }

    public async Task UpdateStatusAsync(IMessagingSender source, string trackingToken, DeliveryResult result)
    {
        using (Telemetry.Activities.StartActivity("MessagingChannel/HandleCallbackAsync"))
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

        using (Telemetry.Activities.StartActivity("MessagingChannel/SendAsync"))
        {
            var job = new MessagingJob(notification, context);

            await userNotificationQueue.ScheduleAsync(
                job.ScheduleKey,
                job,
                job.SendDelay,
                false, ct);
        }
    }

    public Task HandleExceptionAsync(List<MessagingJob> jobs, Exception exception)
    {
        return UpdateAsync(jobs, DeliveryResult.Failed());
    }

    public async Task<bool> HandleAsync(List<MessagingJob> jobs, bool isLastAttempt,
        CancellationToken ct)
    {
        var activityLinks = jobs.SelectMany(x => x.Notification.Links());
        var activityContext = Activity.Current?.Context ?? default;

        using (Telemetry.Activities.StartActivity("MessagingChannel/HandleAsync", ActivityKind.Internal, activityContext, links: activityLinks))
        {
            List<MessagingJob>? unhandledJobs = null;

            foreach (var job in jobs)
            {
                if (await userNotificationStore.IsHandledAsync(job, this, ct))
                {
                    await UpdateAsync(job, DeliveryResult.Skipped());
                }
                else
                {
                    unhandledJobs ??= new List<MessagingJob>();
                    unhandledJobs.Add(job);
                }
            }

            if (unhandledJobs != null)
            {
                await SendJobsAsync(unhandledJobs, ct);
            }

            return true;
        }
    }

    private async Task SendJobsAsync(List<MessagingJob> jobs,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("Send"))
        {
            var lastJob = jobs[^1];

            var commonApp = lastJob.Notification.AppId;
            var commonUser = lastJob.Notification.UserId;

            var app = await appStore.GetCachedAsync(commonApp, ct);

            if (app == null)
            {
                log.LogWarning("Cannot send message: App not found.");

                await UpdateAsync(jobs, DeliveryResult.Handled);
                return;
            }

            var user = await userStore.GetCachedAsync(app.Id, commonUser, ct);

            if (user == null)
            {
                await SkipAsync(jobs, LogMessage.User_Deleted(Name, commonUser));
                return;
            }

            var integrations = integrationManager.Resolve<IMessagingSender>(app, lastJob.Notification).ToList();

            if (integrations.Count == 0)
            {
                await SkipAsync(jobs, LogMessage.Integration_Removed(Name));
                return;
            }

            await UpdateAsync(jobs, DeliveryResult.Attempt);
            try
            {
                var (skip, message) = await BuildMessageAsync(jobs, app, user, ct);

                if (skip != null)
                {
                    await SkipAsync(jobs, skip.Value);
                    return;
                }

                var result = await SendCoreAsync(commonApp, message!, integrations, ct);

                if (result.Status > DeliveryStatus.Attempt)
                {
                    await UpdateAsync(jobs, result);
                }
            }
            catch (DomainException ex)
            {
                await logStore.LogAsync(commonApp, LogMessage.General_Exception(Name, ex));
                throw;
            }
        }
    }

    private async Task<DeliveryResult> SendCoreAsync(string appId, MessagingMessage message, List<ResolvedIntegration<IMessagingSender>> integrations,
        CancellationToken ct)
    {
        var lastResult = default(DeliveryResult);

        foreach (var (_, context, sender) in integrations)
        {
            try
            {
                var result = await sender.SendAsync(context, message, ct);

                // We only sent notifications over the first successful integration.
                if (result.Status >= DeliveryStatus.Sent)
                {
                    break;
                }
            }
            catch (DomainException ex)
            {
                await logStore.LogAsync(appId, LogMessage.General_Exception(sender.Definition.Type, ex));

                // We only expose details of domain exceptions.
                lastResult = DeliveryResult.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                await logStore.LogAsync(appId, LogMessage.General_InternalException(sender.Definition.Type, ex));

                if (sender == integrations[^1].System)
                {
                    throw;
                }

                lastResult = DeliveryResult.Failed();
            }
        }

        return lastResult;
    }

    private async Task SkipAsync(List<MessagingJob> jobs, LogMessage message)
    {
        await logStore.LogAsync(jobs[0].Notification.AppId, message);

        await UpdateAsync(jobs, DeliveryResult.Skipped(message.Reason));
    }

    private async Task UpdateAsync(List<MessagingJob> jobs, DeliveryResult result)
    {
        foreach (var job in jobs)
        {
            await UpdateAsync(job, result);
        }
    }

    private Task UpdateAsync(MessagingJob job, DeliveryResult result)
    {
        return userNotificationStore.TrackAsync(job.AsTrackingKey(Name), result);
    }

    private async Task<(LogMessage? Skip, MessagingMessage?)> BuildMessageAsync(List<MessagingJob> jobs, App app, User user,
        CancellationToken ct)
    {
        var lastJob = jobs[^1];

        foreach (var job in jobs.SkipLast(1))
        {
            lastJob.Notification.ChildNotifications ??= new List<SimpleNotification>();
            lastJob.Notification.ChildNotifications.Add(job.Notification);
        }

        var (skip, template) = await GetTemplateAsync(
            lastJob.Notification.AppId,
            lastJob.Notification.UserLanguage,
            lastJob.Template,
            ct);

        if (skip != default)
        {
            return (skip, null);
        }

        var (text, errors) = messagingFormatter.Format(template, lastJob, app, user);

        // The text can never be null, because we use the subject as default.
        if (errors != null)
        {
            await logStore.LogAsync(app.Id, LogMessage.ChannelTemplate_TemplateError(Name, errors));
        }

        var message = new MessagingMessage
        {
            Targets = lastJob.Configuration,
            // We might also format the text without the template if no primary template is defined.
            Text = text,
        };

        return (default, message.Enrich(lastJob, Name));
    }

    private async Task<(LogMessage? Skip, MessagingTemplate?)> GetTemplateAsync(
        string appId,
        string language,
        string? name,
        CancellationToken ct)
    {
        var (status, template) = await messagingTemplateStore.GetBestAsync(appId, name, language, ct);

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
