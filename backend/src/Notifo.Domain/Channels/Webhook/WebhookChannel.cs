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
using Notifo.Domain.Apps;
using Notifo.Domain.Log;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
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

        public int InitializationOrder => 1000;

        public string Name => Providers.Webhook;

        public bool IsSystem => true;

        public WebhookChannel(IHttpClientFactory httpClientFactory, ISemanticLog log, ILogStore logStore,
            IUserNotificationQueue userNotificationQueue,
            IUserNotificationStore userNotificationStore)
        {
            this.httpClientFactory = httpClientFactory;
            this.log = log;
            this.logStore = logStore;
            this.userNotificationQueue = userNotificationQueue;
            this.userNotificationStore = userNotificationStore;
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            userNotificationQueue.Subscribe(this);

            return Task.CompletedTask;
        }

        public bool CanSend(UserNotification notification, NotificationSetting setting, User user, App app)
        {
            return !string.IsNullOrWhiteSpace(app.WebhookUrl);
        }

        public Task SendAsync(UserNotification notification, NotificationSetting setting, User user, App app, bool isUpdate, CancellationToken ct = default)
        {
            if (!isUpdate)
            {
                return Task.CompletedTask;
            }

            var job = new WebhookJob(notification, app);

            return userNotificationQueue.ScheduleDelayedAsync(
                job.ScheduleKey,
                job,
                0, false, ct);
        }

        public async Task HandleAsync(List<WebhookJob> jobs, bool isLastAttempt, CancellationToken ct)
        {
            foreach (var job in jobs)
            {
                await SendAsync(job, isLastAttempt, ct);
            }
        }

        public Task SendAsync(WebhookJob job, bool isLastAttempt, CancellationToken ct)
        {
            return log.ProfileAsync("SendWebhook", async () =>
            {
                var notification = job.Notification;

                try
                {
                    await UpdateAsync(notification, ProcessStatus.Attempt);

                    using (var client = httpClientFactory.CreateClient())
                    {
                        await client.PostAsJsonAsync(job.Url, job.Notification, ct);
                    }

                    await UpdateAsync(notification, ProcessStatus.Handled);
                }
                catch (Exception ex)
                {
                    if (isLastAttempt)
                    {
                        await UpdateAsync(notification, ProcessStatus.Failed);
                    }

                    await logStore.LogAsync(notification.AppId, $"Webhook: {ex.Message}");

                    throw;
                }
            });
        }

        private Task UpdateAsync(UserNotification notification, ProcessStatus status, string? reason = null)
        {
            return userNotificationStore.CollectAndUpdateAsync(notification, Name, status, reason);
        }
    }
}
