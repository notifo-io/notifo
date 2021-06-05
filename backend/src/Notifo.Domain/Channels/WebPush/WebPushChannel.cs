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
using NodaTime;
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Json;
using Notifo.Infrastructure.Scheduling;
using Squidex.Hosting;
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

        public int Order => 1000;

        public string Name => Providers.WebPush;

        string ISystem.Name => $"Providers({Providers.WebPush})";

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

        public Task InitializeAsync(CancellationToken ct)
        {
            userNotificationQueue.Subscribe(this);

            return Task.CompletedTask;
        }

        public IEnumerable<string> GetConfigurations(UserNotification notification, NotificationSetting settings, SendOptions options)
        {
            if (notification.Silent)
            {
                yield break;
            }

            foreach (var subscription in options.User.WebPushSubscriptions)
            {
                yield return subscription.Endpoint;
            }
        }

        public Task SendAsync(UserNotification notification, NotificationSetting setting, string configuration, SendOptions options, CancellationToken ct)
        {
            var subscription = options.User.WebPushSubscriptions.SingleOrDefault(x => x.Endpoint == configuration);

            if (subscription == null)
            {
                return Task.CompletedTask;
            }

            var job = new WebPushJob(notification, subscription, serializer);

            // Do not use scheduling when the notification is an update.
            if (options.IsUpdate || setting.DelayDuration == Duration.Zero)
            {
                job.IsImmediate = true;

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

        public Task HandleExceptionAsync(WebPushJob job, Exception ex)
        {
            return UpdateAsync(job, job.Subscription.Endpoint, ProcessStatus.Failed);
        }

        public async Task<bool> HandleAsync(WebPushJob job, bool isLastAttempt, CancellationToken ct)
        {
            if (!job.IsImmediate && await userNotificationStore.IsConfirmedOrHandled(job.Id, Name, job.Subscription.Endpoint))
            {
                await UpdateAsync(job, job.Subscription.Endpoint, ProcessStatus.Skipped);
            }
            else
            {
                await SendAsync(job, ct);
            }

            return true;
        }

        private Task SendAsync(WebPushJob job, CancellationToken ct)
        {
            return log.ProfileAsync("SendWebPush", async () =>
            {
                var endpoint = job.Subscription.Endpoint;
                try
                {
                    await UpdateAsync(job, endpoint, ProcessStatus.Attempt);

                    await SendCoreAsync(job, ct);

                    await UpdateAsync(job, endpoint, ProcessStatus.Handled);
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(job.AppId, ex.Message, ct);
                    throw;
                }
            });
        }

        private async Task SendCoreAsync(WebPushJob job, CancellationToken ct)
        {
            try
            {
                var pushSubscription = new PushSubscription(
                    job.Subscription.Endpoint,
                    job.Subscription.Keys["p256dh"],
                    job.Subscription.Keys["auth"]);

                var json = job.Payload;

                await webPushClient.SendNotificationAsync(pushSubscription, json);
            }
            catch (WebPushException ex) when (ex.StatusCode == HttpStatusCode.Gone)
            {
                await logStore.LogAsync(job.AppId, Texts.WebPush_TokenRemoved, ct);

                var command = new RemoveUserWebPushSubscription
                {
                    Subscription = job.Subscription
                };

                await userStore.UpsertAsync(job.AppId, job.UserId, command, ct);
            }
            catch (WebPushException ex)
            {
                throw new DomainException(ex.Message);
            }
        }

        private Task UpdateAsync(WebPushJob job, string endpoint, ProcessStatus status, string? reason = null)
        {
            return userNotificationStore.CollectAndUpdateAsync(job, Name, endpoint, status, reason);
        }
    }
}
