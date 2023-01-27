// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Scheduling;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.Webhook.WebhookJob>;

namespace Notifo.Domain.Channels.Webhook;

public sealed class WebhookChannel : ICommunicationChannel, IScheduleHandler<WebhookJob>
{
    private const string IntegrationId = nameof(IntegrationId);
    private readonly IUserNotificationStore userNotificationStore;
    private readonly IUserNotificationQueue userNotificationQueue;
    private readonly ILogger<WebhookChannel> log;
    private readonly ILogStore logStore;
    private readonly IAppStore appStore;
    private readonly IIntegrationManager integrationManager;

    public string Name => Providers.Webhook;

    public bool IsSystem => true;

    public WebhookChannel(ILogger<WebhookChannel> log, LogStore logStore,
        IAppStore appStore,
        IIntegrationManager integrationManager,
        IUserNotificationQueue userNotificationQueue,
        IUserNotificationStore userNotificationStore)
    {
        this.appStore = appStore;
        this.integrationManager = integrationManager;
        this.log = log;
        this.logStore = logStore;
        this.userNotificationQueue = userNotificationQueue;
        this.userNotificationStore = userNotificationStore;
    }

    public IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelContext context)
    {
        var senders = integrationManager.Resolve<IWebhookSender>(context.App, notification);

        foreach (var (id, _) in senders)
        {
            yield return new SendConfiguration
            {
                [IntegrationId] = id
            };
        }
    }

    public Task HandleExceptionAsync(WebhookJob job, Exception ex)
    {
        return UpdateAsync(job, ProcessStatus.Failed);
    }

    public async Task SendAsync(UserNotification notification, ChannelContext context,
        CancellationToken ct)
    {
        if (!context.Configuration.TryGetValue(IntegrationId, out var integrationId))
        {
            // Old configuration without an integration id.
            return;
        }

        using (Telemetry.Activities.StartActivity("SmsChannel/SendAsync"))
        {
            var job = new WebhookJob(notification, context, integrationId);

            // Do not use scheduling when the notification is an update.
            if (job.IsUpdate)
            {
                await userNotificationQueue.ScheduleAsync(
                    job.ScheduleKey,
                    job,
                    default(Instant),
                    false, ct);
            }
            else
            {
                await userNotificationQueue.ScheduleAsync(
                    job.ScheduleKey,
                    job,
                    job.Delay,
                    false, ct);
            }
        }
    }

    public async Task<bool> HandleAsync(WebhookJob job, bool isLastAttempt,
        CancellationToken ct)
    {
        var activityLinks = job.Notification.Links();
        var activityContext = Activity.Current?.Context ?? default;

        using (Telemetry.Activities.StartActivity("WebhookChannel/HandleAsync", ActivityKind.Internal, activityContext, links: activityLinks))
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

    private async Task SendJobAsync(WebhookJob job,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("Send"))
        {
            var app = await appStore.GetCachedAsync(job.Notification.AppId, ct);

            if (app == null)
            {
                log.LogWarning("Cannot send webhook: App not found.");

                await UpdateAsync(job, ProcessStatus.Handled);
                return;
            }

            var sender = integrationManager.Resolve<IWebhookSender>(job.IntegrationId, app);

            if (sender == null)
            {
                await SkipAsync(job, LogMessage.Integration_Removed(Name));
                return;
            }

            try
            {
                await UpdateAsync(job, ProcessStatus.Attempt);

                await SendCoreAsync(job, sender, ct);

                await UpdateAsync(job, ProcessStatus.Handled);
            }
            catch (Exception ex)
            {
                await logStore.LogAsync(job.Notification.AppId, LogMessage.General_InternalException(Name, ex));
                throw;
            }
        }
    }

    private static Task SendCoreAsync(WebhookJob job, IWebhookSender sender,
        CancellationToken ct)
    {
        var message = new WebhookMessage { Payload = job.Notification };

        return sender.SendAsync(message.Enrich(job), ct);
    }

    private async Task UpdateAsync(WebhookJob job, ProcessStatus status, string? reason = null)
    {
        // We only track the initial publication.
        if (!job.IsUpdate)
        {
            await userNotificationStore.TrackAsync(job.AsTrackingKey(Name), status, reason);
        }
    }

    private async Task SkipAsync(WebhookJob job, LogMessage message)
    {
        await logStore.LogAsync(job.Notification.AppId, message);

        await UpdateAsync(job, ProcessStatus.Skipped, message.Reason);
    }
}
