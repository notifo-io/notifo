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
using IMessagingTemplateStore = Notifo.Domain.ChannelTemplates.IChannelTemplateStore<Notifo.Domain.Channels.Messaging.MessagingTemplate>;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.Messaging.MessagingJob>;

namespace Notifo.Domain.Channels.Messaging;

public sealed class MessagingChannel : ICommunicationChannel, IScheduleHandler<MessagingJob>, IMessagingCallback
{
    private readonly IAppStore appStore;
    private readonly IIntegrationManager integrationManager;
    private readonly ILogger<MessagingChannel> log;
    private readonly ILogStore logStore;
    private readonly IMessagingFormatter messagingFormatter;
    private readonly IMessagingTemplateStore messagingTemplateStore;
    private readonly IUserNotificationQueue userNotificationQueue;
    private readonly IUserNotificationStore userNotificationStore;

    public string Name => Providers.Messaging;

    public MessagingChannel(ILogger<MessagingChannel> log, ILogStore logStore,
        IAppStore appStore,
        IIntegrationManager integrationManager,
        IMessagingFormatter messagingFormatter,
        IMessagingTemplateStore messagingTemplateStore,
        IUserNotificationQueue userNotificationQueue,
        IUserNotificationStore userNotificationStore)
    {
        this.appStore = appStore;
        this.log = log;
        this.logStore = logStore;
        this.integrationManager = integrationManager;
        this.messagingFormatter = messagingFormatter;
        this.messagingTemplateStore = messagingTemplateStore;
        this.userNotificationQueue = userNotificationQueue;
        this.userNotificationStore = userNotificationStore;
    }

    public IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelSetting settings, SendContext context)
    {
        // Faster check because it does not allocate integrations.
        if (!integrationManager.IsConfigured<IMessagingSender>(context.App, notification))
        {
            yield break;
        }

        var senders = integrationManager.Resolve<IMessagingSender>(context.App, notification);

        // Targets are email-addresses or phone-numbers or anything else to identify an user.
        if (senders.Any(x => x.Target.HasTarget(context.User)))
        {
            yield return new SendConfiguration();
        }
    }

    public async Task HandleCallbackAsync(IMessagingSender sender, MessagingCallbackResponse response,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MessagingChannel/HandleCallbackAsync"))
        {
            var (notificationId, result, details) = response;
            var notification = await userNotificationStore.FindAsync(notificationId, ct);

            if (notification != null)
            {
                await UpdateAsync(notification, result, sender, details);
            }

            userNotificationQueue.Complete(MessagingJob.ComputeScheduleKey(notificationId));
        }
    }

    private async Task UpdateAsync(UserNotification notification, MessagingResult result, IMessagingSender sender, string? details)
    {
        if (!notification.Channels.TryGetValue(Name, out var channel))
        {
            // There is no activity on this channel.
            return;
        }

        if (channel.Status.Count == 0)
        {
            return;
        }

        // We create only one configuration for this channel. Therefore it must be the first.
        var (configurationId, status) = channel.Status.First();

        if (status.Status == ProcessStatus.Attempt)
        {
            var identifier = TrackingKey.ForNotification(notification, Name, configurationId);

            if (result == MessagingResult.Delivered)
            {
                await userNotificationStore.TrackAsync(identifier, ProcessStatus.Handled);
            }
            else if (result == MessagingResult.Failed)
            {
                var message = LogMessage.Sms_CallbackError(sender.Name, details);

                // Also log the error to the app log.
                await logStore.LogAsync(notification.AppId, message);

                await userNotificationStore.TrackAsync(identifier, ProcessStatus.Failed);
            }
        }
    }

    public async Task SendAsync(UserNotification notification, ChannelSetting setting, Guid configurationId, SendConfiguration configuration, SendContext context,
        CancellationToken ct)
    {
        if (context.IsUpdate)
        {
            return;
        }

        using (Telemetry.Activities.StartActivity("MessagingChannel/SendAsync"))
        {
            var job = new MessagingJob(notification, setting, configurationId);

            var integrations = integrationManager.Resolve<IMessagingSender>(context.App, notification);

            // We try all integrations, ordered by priority.
            foreach (var (_, sender) in integrations)
            {
                await sender.AddTargetsAsync(job, context.User);
            }

            // Should not happen because we check before if there is at least one target.
            if (job.Targets.Count == 0)
            {
                await UpdateAsync(job, ProcessStatus.Skipped);
            }

            await userNotificationQueue.ScheduleAsync(
                job.ScheduleKey,
                job,
                job.Delay,
                false, ct);
        }
    }

    public Task HandleExceptionAsync(MessagingJob job, Exception ex)
    {
        return UpdateAsync(job, ProcessStatus.Failed);
    }

    public async Task<bool> HandleAsync(MessagingJob job, bool isLastAttempt,
        CancellationToken ct)
    {
        var links = job.Notification.Links();

        var parentContext = Activity.Current?.Context ?? default;

        using (Telemetry.Activities.StartActivity("MessagingChannel/HandleAsync", ActivityKind.Internal, parentContext, links: links))
        {
            if (await userNotificationStore.IsHandledAsync(job, this, ct))
            {
                await UpdateAsync(job, ProcessStatus.Skipped);
            }
            else
            {
                await SendJobAsync(job, ct);
            }

            return true;
        }
    }

    private async Task SendJobAsync(MessagingJob job,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("Send"))
        {
            var app = await appStore.GetCachedAsync(job.Notification.AppId, ct);

            if (app == null)
            {
                log.LogWarning("Cannot send message: App not found.");

                await UpdateAsync(job, ProcessStatus.Handled);
                return;
            }

            try
            {
                await UpdateAsync(job, ProcessStatus.Attempt);

                var senders = integrationManager.Resolve<IMessagingSender>(app, job.Notification).Select(x => x.Target).ToList();

                if (senders.Count == 0)
                {
                    await SkipAsync(job, LogMessage.Integration_Removed(Name));
                    return;
                }

                var (skip, template) = await GetTemplateAsync(
                    job.Notification.AppId,
                    job.Notification.UserLanguage,
                    job.NotificationTemplate,
                    ct);

                if (skip != null)
                {
                    await SkipAsync(job, skip.Value);
                    return;
                }

                var text = messagingFormatter.Format(template, job.Notification);

                await SendCoreAsync(job, text, senders, ct);
            }
            catch (DomainException ex)
            {
                await logStore.LogAsync(job.Notification.AppId, LogMessage.General_Exception(Name, ex));
                throw;
            }
        }
    }

    private async Task SendCoreAsync(MessagingJob job, string text, List<IMessagingSender> senders,
        CancellationToken ct)
    {
        var lastSender = senders[^1];

        foreach (var sender in senders)
        {
            try
            {
                var result = await sender.SendAsync(job, text, ct);

                // If the message has been delivered, we do not try other integrations.
                if (result == MessagingResult.Delivered)
                {
                    await UpdateAsync(job, ProcessStatus.Handled);
                    return;
                }
            }
            catch (DomainException ex)
            {
                await logStore.LogAsync(job.Notification.AppId, LogMessage.General_Exception(Name, ex));

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

    private Task UpdateAsync(MessagingJob job, ProcessStatus status, string? reason = null)
    {
        return userNotificationStore.TrackAsync(job.Tracking, status, reason);
    }

    private async Task SkipAsync(MessagingJob job, LogMessage message)
    {
        await logStore.LogAsync(job.Notification.AppId, message);

        await UpdateAsync(job, ProcessStatus.Skipped, message.Reason);
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
