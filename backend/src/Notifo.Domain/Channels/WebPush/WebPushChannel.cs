// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
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
        private const string Endpoint = nameof(Endpoint);
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

        public IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelSetting settings, SendContext context)
        {
            if (notification.Silent)
            {
                yield break;
            }

            foreach (var subscription in context.User.WebPushSubscriptions)
            {
                if (!string.IsNullOrWhiteSpace(subscription.Endpoint) &&
                    subscription.Keys.ContainsKey("p256dh") &&
                    subscription.Keys.ContainsKey("auth"))
                {
                    yield return new SendConfiguration
                    {
                        [Endpoint] = subscription.Endpoint
                    };
                }
            }
        }

        public async Task SendAsync(UserNotification notification, ChannelSetting setting, Guid configurationId, SendConfiguration properties, SendContext context,
            CancellationToken ct)
        {
            if (!properties.TryGetValue(Endpoint, out var endpoint))
            {
                // Old configuration without a mobile push token.
                return;
            }

            using (Telemetry.Activities.StartActivity("WebPushChannel/SendAsync"))
            {
                var subscription = context.User.WebPushSubscriptions.FirstOrDefault(x => x.Endpoint == endpoint);

                if (subscription == null)
                {
                    // Subscription has been removed in the meantime.
                    return;
                }

                var job = new WebPushJob(notification, setting, configurationId, subscription, serializer, context.IsUpdate);

                // Do not use scheduling when the notification is an update.
                if (context.IsUpdate)
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

        public Task HandleExceptionAsync(WebPushJob job, Exception ex)
        {
            return UpdateAsync(job, ProcessStatus.Failed);
        }

        public async Task<bool> HandleAsync(WebPushJob job, bool isLastAttempt,
            CancellationToken ct)
        {
            var links = job.Links();

            var parentContext = Activity.Current?.Context ?? default;

            using (Telemetry.Activities.StartActivity("WebPushChannel/HandleAsync", ActivityKind.Internal, parentContext, links: links))
            {
                if (await userNotificationStore.IsHandledAsync(job, this, ct))
                {
                    await UpdateAsync(job, ProcessStatus.Skipped);
                }
                else
                {
                    await SendAsync(job, ct);
                }

                return true;
            }
        }

        private async Task SendAsync(WebPushJob job,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("Send"))
            {
                try
                {
                    await UpdateAsync(job, ProcessStatus.Attempt);

                    await SendCoreAsync(job, ct);

                    await UpdateAsync(job, ProcessStatus.Handled);
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(job.Tracking.AppId, Name, ex.Message);
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
                await logStore.LogAsync(job.Tracking.AppId, Name, Texts.WebPush_TokenRemoved);

                var command = new RemoveUserWebPushSubscription
                {
                    Endpoint = job.Subscription.Endpoint
                };

                await userStore.UpsertAsync(job.Tracking.AppId, job.Tracking.UserId, command, ct);
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
                await userNotificationStore.TrackAsync(job.Tracking, status, reason);
            }
        }
    }
}
