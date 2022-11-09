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
using Notifo.Domain.Resources;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Scheduling;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.MobilePush.MobilePushJob>;

namespace Notifo.Domain.Channels.MobilePush
{
    public sealed class MobilePushChannel : ICommunicationChannel, IScheduleHandler<MobilePushJob>
    {
        private const string MobilePushToken = nameof(MobilePushToken);
        private readonly IAppStore appStore;
        private readonly IClock clock;
        private readonly IIntegrationManager integrationManager;
        private readonly ILogger<MobilePushChannel> log;
        private readonly ILogStore logStore;
        private readonly IUserNotificationQueue userNotificationQueue;
        private readonly IUserNotificationStore userNotificationStore;
        private readonly IUserStore userStore;

        public string Name => Providers.MobilePush;

        public MobilePushChannel(ILogger<MobilePushChannel> log, ILogStore logStore,
            IAppStore appStore,
            IIntegrationManager integrationManager,
            IUserNotificationQueue userNotificationQueue,
            IUserNotificationStore userNotificationStore,
            IUserStore userStore,
            IClock clock)
        {
            this.appStore = appStore;
            this.log = log;
            this.logStore = logStore;
            this.integrationManager = integrationManager;
            this.userNotificationQueue = userNotificationQueue;
            this.userNotificationStore = userNotificationStore;
            this.userStore = userStore;
            this.clock = clock;
        }

        public IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelSetting settings, SendContext context)
        {
            if (!integrationManager.IsConfigured<IMobilePushSender>(context.App, notification))
            {
                yield break;
            }

            foreach (var token in context.User.MobilePushTokens)
            {
                if (!string.IsNullOrWhiteSpace(token.Token))
                {
                    yield return new SendConfiguration
                    {
                        [MobilePushToken] = token.Token
                    };
                }
            }
        }

        public async Task HandleSeenAsync(TrackingToken token)
        {
            using (Telemetry.Activities.StartActivity("MobilePushChannel/HandleSeenAsync"))
            {
                var configurationId = token.ConfigurationId;

                if (configurationId == default)
                {
                    // Old tracking code.
                    return;
                }

                var notification = await userNotificationStore.FindAsync(token.NotificationId);

                if (notification == null)
                {
                    // The notification does not exist anymore.
                    return;
                }

                if (!notification.Channels.TryGetValue(Name, out var channel))
                {
                    // There is no activity on this channel.
                    return;
                }

                if (!channel.Status.TryGetValue(configurationId, out var status))
                {
                    // The configuration ID does not match.
                    return;
                }

                if (status.Configuration?.TryGetValue(MobilePushToken, out var mobileToken) != true)
                {
                    // The configuration has no token.
                    return;
                }

                var user = await userStore.GetCachedAsync(notification.AppId, notification.UserId);

                if (user == null)
                {
                    // The user or app does not exist anymore.
                    return;
                }

                var userToken = user.MobilePushTokens.FirstOrDefault(x => x.Token == mobileToken && x.DeviceType == MobileDeviceType.iOS);

                if (userToken == null)
                {
                    // The token is not for iOS or has been removed.
                    return;
                }

                await TryWakeupAsync(notification, configurationId, userToken, default);
            }
        }

        public async Task SendAsync(UserNotification notification, ChannelSetting setting, Guid configurationId, SendConfiguration properties, SendContext context,
            CancellationToken ct)
        {
            if (!properties.TryGetValue(MobilePushToken, out var tokenString))
            {
                // Old configuration without a mobile push token.
                return;
            }

            using (Telemetry.Activities.StartActivity("MobilePushChannel/SendAsync"))
            {
                var token = context.User.MobilePushTokens.SingleOrDefault(x => x.Token == tokenString);

                if (token == null)
                {
                    // Token has been deleted in the meantime.
                    return;
                }

                if (token.DeviceType == MobileDeviceType.iOS)
                {
                    await TryWakeupAsync(notification, configurationId, token, ct);
                }

                var job = new MobilePushJob(notification, setting, configurationId, token.Token, token.DeviceType, context.IsUpdate);

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

        private async Task TryWakeupAsync(UserNotification notification, Guid configurationId, MobilePushToken token,
            CancellationToken ct)
        {
            var nextWakeup = token.GetNextWakeupTime(clock);

            if (nextWakeup == null)
            {
                return;
            }

            var dummyNotification = new UserNotification
            {
                AppId = notification.AppId,
                UserId = notification.UserId,
                UserLanguage = notification.UserLanguage
            };

            var wakeupJob = new MobilePushJob(dummyNotification, null, configurationId, token.Token, token.DeviceType, false);

            await userNotificationQueue.ScheduleAsync(
                wakeupJob.ScheduleKey,
                wakeupJob,
                nextWakeup.Value,
                false, ct);

            try
            {
                var command = new UpdateMobileWakeupTime
                {
                    Token = token.Token,
                    Timestamp = nextWakeup.Value
                };

                await userStore.UpsertAsync(notification.AppId, notification.UserId, command, ct);
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Failed to wakeup device.");
            }
        }

        public async Task<bool> HandleAsync(MobilePushJob job, bool isLastAttempt,
            CancellationToken ct)
        {
            var links = job.Notification.Links();

            var parentContext = Activity.Current?.Context ?? default;

            using (Telemetry.Activities.StartActivity("MobilePushChannel/HandleAsync", ActivityKind.Internal, parentContext, links: links))
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

        public Task HandleExceptionAsync(MobilePushJob job, Exception ex)
        {
            return UpdateAsync(job, ProcessStatus.Failed);
        }

        private async Task SendJobAsync(MobilePushJob job,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("SendMobilePush"))
            {
                var notification = job.Notification;

                var app = await appStore.GetCachedAsync(notification.AppId, ct);

                if (app == null)
                {
                    log.LogWarning("Cannot send mobile push: App not found.");

                    await UpdateAsync(job, ProcessStatus.Handled);
                    return;
                }

                try
                {
                    await UpdateAsync(job, ProcessStatus.Attempt);

                    var senders = integrationManager.Resolve<IMobilePushSender>(app, notification).Select(x => x.Target).ToList();

                    if (senders.Count == 0)
                    {
                        await SkipAsync(job, Texts.MobilePush_ConfigReset);
                        return;
                    }

                    await SendCoreAsync(job, app, senders, ct);

                    await UpdateAsync(job, ProcessStatus.Handled);
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(app.Id, Name, ex.Message);
                    throw;
                }
            }
        }

        private async Task SendCoreAsync(MobilePushJob job, App app, List<IMobilePushSender> senders,
            CancellationToken ct)
        {
            var lastSender = senders[^1];

            var notification = job.Notification;

            foreach (var sender in senders)
            {
                try
                {
                    var options = new MobilePushOptions
                    {
                        IsConfirmed = job.IsConfirmed,
                        DeviceType = job.DeviceType,
                        DeviceToken = job.DeviceToken,
                        Wakeup = notification.Formatting == null
                    };

                    await sender.SendAsync(notification, options, ct);
                    return;
                }
                catch (MobilePushTokenExpiredException)
                {
                    await logStore.LogAsync(app.Id, sender.Name, Texts.MobilePush_TokenRemoved);

                    var command = new RemoveUserMobileToken
                    {
                        Token = job.DeviceToken
                    };

                    await userStore.UpsertAsync(app.Id, notification.UserId, command, ct);
                    break;
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(app.Id, sender.Name, ex.Message);

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

        private async Task UpdateAsync(MobilePushJob job, ProcessStatus status, string? reason = null)
        {
            // We only track the initial publication.
            if (!job.IsUpdate)
            {
                await userNotificationStore.TrackAsync(job.Tracking, status, reason);
            }
        }

        private async Task SkipAsync(MobilePushJob job, string reason)
        {
            await logStore.LogAsync(job.Notification.AppId, Name, reason);

            await UpdateAsync(job, ProcessStatus.Skipped);
        }
    }
}
