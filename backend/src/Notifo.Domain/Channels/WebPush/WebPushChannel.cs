// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using Microsoft.Extensions.Options;
using NodaTime;
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Json;
using Notifo.Infrastructure.Scheduling;
using WebPush;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.WebPush.WebPushJob>;

namespace Notifo.Domain.Channels.WebPush
{
    public sealed class WebPushChannel : ICommunicationChannel, IScheduleHandler<WebPushJob>, IWebPushService
    {
        private readonly WebPushClient webPushClient = new WebPushClient();
        private readonly IJsonSerializer serializer;
        private readonly ILogStore logStore;
        private readonly IUserNotificationStore userNotificationStore;
        private readonly IUserStore userStore;
        private readonly IUserNotificationQueue userNotificationQueue;

        public string Name => Providers.WebPush;

        public string PublicKey { get; }

        public WebPushChannel(ILogStore logStore, IOptions<WebPushOptions> options,
            IUserNotificationQueue userNotificationQueue,
            IUserNotificationStore userNotificationStore,
            IUserStore userStore,
            IJsonSerializer serializer)
        {
            this.serializer = serializer;
            this.userNotificationQueue = userNotificationQueue;
            this.userNotificationStore = userNotificationStore;
            this.userStore = userStore;
            this.logStore = logStore;

            webPushClient.SetVapidDetails(
                options.Value.Subject,
                options.Value.VapidPublicKey,
                options.Value.VapidPrivateKey);

            PublicKey = options.Value.VapidPublicKey;
        }

        public IEnumerable<string> GetConfigurations(UserNotification notification, NotificationSetting settings, SendOptions options)
        {
            if (notification.Silent)
            {
                yield break;
            }

            foreach (var subscription in options.User.WebPushSubscriptions)
            {
                if (!string.IsNullOrWhiteSpace(subscription.Endpoint))
                {
                    yield return subscription.Endpoint;
                }
            }
        }

        public Task SendAsync(UserNotification notification, NotificationSetting setting, string configuration, SendOptions options,
            CancellationToken ct)
        {
            var subscription = options.User.WebPushSubscriptions.FirstOrDefault(x => x.Endpoint == configuration);

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
            return UpdateAsync(job, ProcessStatus.Failed);
        }

        public async Task<bool> HandleAsync(WebPushJob job, bool isLastAttempt,
            CancellationToken ct)
        {
            var config = job.Subscription.Endpoint;

            // If the notification is not scheduled it is very unlikey it has been confirmed already.
            if (!job.IsImmediate && await userNotificationStore.IsConfirmedOrHandledAsync(job.Id, config, Name, ct))
            {
                await UpdateAsync(job, ProcessStatus.Skipped);
            }
            else
            {
                await SendAsync(job, ct);
            }

            return true;
        }

        private async Task SendAsync(WebPushJob job,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("SendWebPush"))
            {
                try
                {
                    await UpdateAsync(job, ProcessStatus.Attempt);

                    await SendCoreAsync(job, ct);

                    await UpdateAsync(job, ProcessStatus.Handled);
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(job.AppId, Name, ex.Message);
                    throw;
                }
            }
        }

        private async Task SendCoreAsync(WebPushJob job,
            CancellationToken ct)
        {
            try
            {
                var pushSubscription = new PushSubscription(
                    job.Subscription.Endpoint,
                    job.Subscription.Keys["p256dh"],
                    job.Subscription.Keys["auth"]);

                var json = job.Payload;

                await webPushClient.SendNotificationAsync(pushSubscription, json, cancellationToken: ct);
            }
            catch (WebPushException ex) when (ex.StatusCode == HttpStatusCode.Gone)
            {
                await logStore.LogAsync(job.AppId, Name, Texts.WebPush_TokenRemoved);

                var command = new RemoveUserWebPushSubscription
                {
                    Endpoint = job.Subscription.Endpoint
                };

                await userStore.UpsertAsync(job.AppId, job.UserId, command, ct);
            }
            catch (WebPushException ex)
            {
                throw new DomainException(ex.Message);
            }
        }

        private async Task UpdateAsync(WebPushJob job, ProcessStatus status, string? reason = null)
        {
            // We only track the initial publication.
            if (!job.IsUpdate)
            {
                await userNotificationStore.CollectAndUpdateAsync(job, Name, job.Subscription.Endpoint, status, reason);
            }
        }
    }
}
