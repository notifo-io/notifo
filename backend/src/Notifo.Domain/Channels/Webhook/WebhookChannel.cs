// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using System.Net.Http.Json;
using NodaTime;
using Notifo.Domain.Channels.Webhook.Integrations;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Scheduling;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.Webhook.WebhookJob>;

namespace Notifo.Domain.Channels.Webhook;

public sealed class WebhookChannel : ICommunicationChannel, IScheduleHandler<WebhookJob>
{
    private const string WebhookId = nameof(WebhookId);
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IUserNotificationStore userNotificationStore;
    private readonly IUserNotificationQueue userNotificationQueue;
    private readonly ILogStore logStore;
    private readonly IIntegrationManager integrationManager;

    public string Name => Providers.Webhook;

    public bool IsSystem => true;

    public WebhookChannel(IHttpClientFactory httpClientFactory, ILogStore logStore,
        IIntegrationManager integrationManager,
        IUserNotificationQueue userNotificationQueue,
        IUserNotificationStore userNotificationStore)
    {
        this.httpClientFactory = httpClientFactory;
        this.logStore = logStore;
        this.integrationManager = integrationManager;
        this.userNotificationQueue = userNotificationQueue;
        this.userNotificationStore = userNotificationStore;
    }

    public IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelSetting setting, SendContext context)
    {
        var webhooks = integrationManager.Resolve<WebhookDefinition>(context.App, notification);

        foreach (var (id, _) in webhooks)
        {
            yield return new SendConfiguration
            {
                [WebhookId] = id
            };
        }
    }

    public Task HandleExceptionAsync(WebhookJob job, Exception ex)
    {
        return UpdateAsync(job, ProcessStatus.Failed);
    }

    public async Task SendAsync(UserNotification notification, ChannelSetting setting, Guid configurationId, SendConfiguration properties, SendContext context,
        CancellationToken ct)
    {
        if (!properties.TryGetValue("WebhookId", out var webhookId))
        {
            // Old configuration without a mobile push token.
            return;
        }

        using (Telemetry.Activities.StartActivity("SmsChannel/SendAsync"))
        {
            var webhook = integrationManager.Resolve<WebhookDefinition>(webhookId, context.App, notification);

            // The webhook must match the name or the conditions.
            if (webhook == null || !ShouldSend(webhook, context.IsUpdate, setting.Template))
            {
                return;
            }

            var job = new WebhookJob(notification, setting, configurationId, webhook, context.IsUpdate);

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
        try
        {
            await UpdateAsync(job, ProcessStatus.Attempt);

            await SendCoreAsync(job, ct);

            await UpdateAsync(job, ProcessStatus.Handled);
        }
        catch (Exception ex)
        {
            await logStore.LogAsync(job.Notification.AppId, LogMessage.General_InternalException(Name, ex));
            throw;
        }
    }

    private async Task SendCoreAsync(WebhookJob job,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("Send"))
        {
            using (var client = httpClientFactory.CreateClient())
            {
                var request = new HttpRequestMessage(new HttpMethod(job.Webhook.HttpMethod), job.Webhook.HttpUrl)
                {
                    Content = JsonContent.Create(job.Notification)
                };

                await client.SendAsync(request, ct);
            }
        }
    }

    private async Task UpdateAsync(WebhookJob job, ProcessStatus status, string? reason = null)
    {
        // We only track the initial publication.
        if (!job.IsUpdate)
        {
            await userNotificationStore.TrackAsync(job.Tracking, status, reason);
        }
    }

    private static bool ShouldSend(WebhookDefinition webhook, bool isUpdate, string? name)
    {
        if (webhook.SendAlways)
        {
            return true;
        }

        if (isUpdate)
        {
            return webhook.SendConfirm;
        }
        else
        {
            return !string.IsNullOrWhiteSpace(name) && string.Equals(name, webhook.Name, StringComparison.Ordinal);
        }
    }
}
