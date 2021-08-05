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
using Notifo.Domain.Utils;
using Notifo.Infrastructure.Scheduling;
using Squidex.Hosting;
using Squidex.Log;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.Webhook.WebhookJob>;

namespace Notifo.Domain.Channels.Webhook
{
    public sealed class WebhookChannel : ICommunicationChannel, IScheduleHandler<WebhookJob>, IInitializable
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IUserNotificationStore userNotificationStore;
        private readonly IUserNotificationQueue userNotificationQueue;
        private readonly ISemanticLog log;
        private readonly ILogStore logStore;
        private readonly IIntegrationManager integrationManager;

        public int Order => 1000;

        public string Name => Providers.Webhook;

        string ISystem.Name => $"Providers({Providers.Webhook})";

        public bool IsSystem => true;

        public WebhookChannel(IHttpClientFactory httpClientFactory, ISemanticLog log, ILogStore logStore,
            IIntegrationManager integrationManager,
            IUserNotificationQueue userNotificationQueue,
            IUserNotificationStore userNotificationStore)
        {
            this.httpClientFactory = httpClientFactory;
            this.log = log;
            this.logStore = logStore;
            this.integrationManager = integrationManager;
            this.userNotificationQueue = userNotificationQueue;
            this.userNotificationStore = userNotificationStore;
        }

        public Task InitializeAsync(CancellationToken ct)
        {
            userNotificationQueue.Subscribe(this);

            return Task.CompletedTask;
        }

        public IEnumerable<string> GetConfigurations(UserNotification notification, NotificationSetting setting, SendOptions options)
        {
            var webhooks = integrationManager.Resolve<WebhookDefinition>(options.App, notification.Test);

            foreach (var webhook in webhooks)
            {
                yield return webhook.Url;
            }
        }

        public Task HandleExceptionAsync(WebhookJob job, Exception ex)
        {
            return UpdateAsync(job.Notification, job.Url, ProcessStatus.Failed);
        }

        public Task SendAsync(UserNotification notification, NotificationSetting setting, string configuration, SendOptions options,
            CancellationToken ct)
        {
            var job = new WebhookJob(notification, configuration);

            return userNotificationQueue.ScheduleAsync(
                job.ScheduleKey,
                job,
                Duration.Zero,
                false, ct);
        }

        public async Task<bool> HandleAsync(WebhookJob job, bool isLastAttempt,
            CancellationToken ct)
        {
            await log.ProfileAsync("SendWebhook", async () =>
            {
                var url = job.Url;
                try
                {
                    await UpdateAsync(job.Notification, url, ProcessStatus.Attempt);

                    await SendCoreAsync(job, url, ct);

                    await UpdateAsync(job.Notification, url, ProcessStatus.Handled);
                }
                catch (Exception ex)
                {
                    await logStore.LogAsync(job.Notification.AppId, $"Webhook: {ex.Message}", ct);
                    throw;
                }
            });

            return true;
        }

        private async Task SendCoreAsync(WebhookJob job, string url,
            CancellationToken ct)
        {
            using (var client = httpClientFactory.CreateClient())
            {
                await client.PostAsJsonAsync(url, job.Notification, ct);
            }
        }

        private Task UpdateAsync(UserNotification notification, string url, ProcessStatus status, string? reason = null)
        {
            return userNotificationStore.CollectAndUpdateAsync(notification, Name, url, status, reason);
        }
    }
}
