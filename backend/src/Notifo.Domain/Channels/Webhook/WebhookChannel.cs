// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Notifo.Domain.Channels.Webhook.Integrations;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Scheduling;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.Webhook.WebhookJob>;

namespace Notifo.Domain.Channels.Webhook
{
    public sealed class WebhookChannel : ICommunicationChannel, IScheduleHandler<WebhookJob>
    {
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

        public IEnumerable<string> GetConfigurations(UserNotification notification, NotificationSetting setting, SendOptions options)
        {
            var webhooks = integrationManager.Resolve<WebhookDefinition>(options.App, notification.Test);

            foreach (var (id, _) in webhooks)
            {
                yield return id;
            }
        }

        public Task HandleExceptionAsync(WebhookJob job, Exception ex)
        {
            return UpdateAsync(job, ProcessStatus.Failed);
        }

        public Task SendAsync(UserNotification notification, NotificationSetting setting, string configuration, SendOptions options,
            CancellationToken ct)
        {
            var webhook = integrationManager.Resolve<WebhookDefinition>(configuration, options.App, notification.Test);

            // The webhook must match the name or the conditions.
            if (webhook == null || !ShouldSend(webhook, options.IsUpdate, setting.Template))
            {
                return Task.CompletedTask;
            }

            var job = new WebhookJob(notification, configuration, webhook)
            {
                IsUpdate = options.IsUpdate
            };

            // Do not use scheduling when the notification is an update.
            if (options.IsUpdate)
            {
                return userNotificationQueue.ScheduleAsync(
                    job.ScheduleKey,
                    job,
                    default(Instant),
                    false, ct);
            }
            else
            {
                return userNotificationQueue.ScheduleAsync(
                    job.ScheduleKey,
                    job,
                    setting.DelayDuration,
                    false, ct);
            }
        }

        public async Task<bool> HandleAsync(WebhookJob job, bool isLastAttempt,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("SendWebhook"))
            {
                try
                {
                    await UpdateAsync(job, ProcessStatus.Attempt);

                    await SendCoreAsync(job, ct);

                    await UpdateAsync(job, ProcessStatus.Handled);
                }
                catch (Exception ex)
                {
                    await logStore.LogAsync(job.Notification.AppId, Name, ex.Message);
                    throw;
                }
            }

            return true;
        }

        private async Task SendCoreAsync(WebhookJob job,
            CancellationToken ct)
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

        private async Task UpdateAsync(WebhookJob job, ProcessStatus status, string? reason = null)
        {
            // We only track the initial publication.
            if (!job.IsUpdate)
            {
                await userNotificationStore.CollectAndUpdateAsync(job.Notification, Name, job.WebhookId, status, reason);
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
                return !string.IsNullOrWhiteSpace(name) && string.Equals(name, webhook.Name);
            }
        }
    }
}
