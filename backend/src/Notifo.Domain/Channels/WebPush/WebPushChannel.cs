// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Notifo.Domain.Apps;
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Initialization;
using Notifo.Infrastructure.Json;
using Notifo.Infrastructure.Scheduling;
using Squidex.Log;
using WebPush;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.WebPush.WebPushJob>;

namespace Notifo.Domain.Channels.WebPush
{
    public sealed class WebPushChannel : ICommunicationChannel, IScheduleHandler<WebPushJob>, IWebPushService, IInitializable
    {
        private readonly WebPushClient webPushClient = new WebPushClient();
        private readonly IJsonSerializer serializer;
        private readonly ILogStore logStore;
        private readonly IUserNotificationStore userNotificationStore;
        private readonly IUserStore userStore;
        private readonly IUserNotificationQueue userNotificationQueue;
        private readonly ISemanticLog log;

        public int InitializationOrder => 1000;

        public string Name => Providers.WebPush;

        public string PublicKey { get; }

        public WebPushChannel(ILogStore logStore, ISemanticLog log, IOptions<WebPushOptions> options,
            IUserNotificationQueue userNotificationQueue,
            IUserNotificationStore userNotificationStore,
            IUserStore userStore,
            IJsonSerializer serializer)
        {
            this.log = log;
            this.logStore = logStore;
            this.serializer = serializer;
            this.userNotificationQueue = userNotificationQueue;
            this.userNotificationStore = userNotificationStore;
            this.userStore = userStore;

            webPushClient.SetVapidDetails(
                options.Value.Subject,
                options.Value.VapidPublicKey,
                options.Value.VapidPrivateKey);

            PublicKey = options.Value.VapidPublicKey;
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            userNotificationQueue.Subscribe(this);

            return Task.CompletedTask;
        }

        public bool CanSend(UserNotification notification, NotificationSetting setting, User user, App app)
        {
            return !notification.Silent && user.WebPushSubscriptions.Count > 0;
        }

        public Task SendAsync(UserNotification notification, NotificationSetting setting, User user, App app, bool isUpdate, CancellationToken ct = default)
        {
            var job = new WebPushJob(notification, user, serializer);

            return userNotificationQueue.ScheduleDelayedAsync(
                job.ScheduleKey,
                job,
                setting.DelayInSecondsOrZero,
                false, ct);
        }

        public async Task HandleAsync(List<WebPushJob> jobs, bool isLastAttempt, CancellationToken ct)
        {
            foreach (var job in jobs)
            {
                if (await userNotificationStore.IsConfirmedOrHandled(job.Id, Name))
                {
                    await UpdateAsync(job, ProcessStatus.Skipped);
                }
                else
                {
                    await SendAsync(job, isLastAttempt, ct);
                }
            }
        }

        public Task SendAsync(WebPushJob job, bool isLastAttempt, CancellationToken ct)
        {
            return log.ProfileAsync("SendWebPush", async () =>
            {
                try
                {
                    await UpdateAsync(job, ProcessStatus.Attempt);

                    await SendAnyAsync(job, ct);

                    await UpdateAsync(job, ProcessStatus.Handled);
                }
                catch (Exception ex)
                {
                    if (isLastAttempt)
                    {
                        await UpdateAsync(job, ProcessStatus.Failed);
                    }

                    if (ex is DomainException domainException)
                    {
                        await logStore.LogAsync(job.AppId, domainException.Message);
                    }
                    else
                    {
                        throw;
                    }
                }
            });
        }

        private async Task SendAnyAsync(WebPushJob task, CancellationToken ct)
        {
            var tasks = task.Subscriptions.Select(s => SendAsync(task, s, ct));
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                if (tasks.Any(x => x.Status == TaskStatus.RanToCompletion))
                {
                    log.LogWarning(ex, w =>
                    {
                        w.WriteProperty("action", "SendWebPush");
                        w.WriteProperty("status", "PartialFailure");

                        Profiler.Session?.Write(w);
                    });
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task SendAsync(WebPushJob task, WebPushSubscription subscription, CancellationToken ct)
        {
            try
            {
                var pushSubscription = new PushSubscription(
                    subscription.Endpoint,
                    subscription.Keys["p256dh"],
                    subscription.Keys["auth"]);

                var json = task.Payload;

                await webPushClient.SendNotificationAsync(pushSubscription, json);
            }
            catch (WebPushException ex) when (ex.StatusCode == HttpStatusCode.Gone)
            {
                await logStore.LogAsync(task.AppId, Texts.MobilePush_TokenRemoved, ct);

                var command = new RemoveUserWebPushSubscription
                {
                    Subscription = subscription
                };

                await userStore.UpsertAsync(task.AppId, task.UserId, command, ct);
            }
        }

        private Task UpdateAsync(WebPushJob job, ProcessStatus status, string? reason = null)
        {
            return userNotificationStore.CollectAndUpdateAsync(job, Name, status, reason);
        }
    }
}
