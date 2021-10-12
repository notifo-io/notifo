// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Scheduling;
using Squidex.Log;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.MobilePush.MobilePushJob>;

namespace Notifo.Domain.Channels.MobilePush
{
    public sealed class MobilePushChannel : ICommunicationChannel, IScheduleHandler<MobilePushJob>
    {
        private readonly IAppStore appStore;
        private readonly IIntegrationManager integrationManager;
        private readonly ILogStore logStore;
        private readonly ISemanticLog log;
        private readonly IUserNotificationQueue userNotificationQueue;
        private readonly IUserNotificationStore userNotificationStore;
        private readonly IUserStore userStore;
        private readonly IClock clock;

        public string Name => Providers.MobilePush;

        public MobilePushChannel(ISemanticLog log, ILogStore logStore,
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

        public IEnumerable<string> GetConfigurations(UserNotification notification, NotificationSetting settings, SendOptions options)
        {
            if (!integrationManager.IsConfigured<IMobilePushSender>(options.App, notification.Test))
            {
                yield break;
            }

            foreach (var token in options.User.MobilePushTokens)
            {
                if (!string.IsNullOrWhiteSpace(token.Token))
                {
                    yield return token.Token;
                }
            }
        }

        public async Task HandleSeenAsync(Guid id, TrackingDetails details)
        {
            var token = details.DeviceIdentifier;

            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            var notification = await userNotificationStore.FindAsync(id);

            if (notification == null)
            {
                return;
            }

            var user = await userStore.GetCachedAsync(notification.AppId, notification.UserId);

            if (user == null)
            {
                return;
            }

            var userToken = user.MobilePushTokens.FirstOrDefault(x => x.Token == token && x.DeviceType == MobileDeviceType.iOS);

            if (userToken != null)
            {
                await TryWakeupAsync(notification, userToken, default);
            }
        }

        public async Task SendAsync(UserNotification notification, NotificationSetting setting, string configuration, SendOptions options,
            CancellationToken ct)
        {
            var token = options.User.MobilePushTokens.SingleOrDefault(x => x.Token == configuration);

            if (token == null)
            {
                return;
            }

            if (token.DeviceType == MobileDeviceType.iOS)
            {
                await TryWakeupAsync(notification, token, ct);
            }

            var job = new MobilePushJob(notification, configuration, token.DeviceType)
            {
                IsImmediate = options.IsUpdate || setting.DelayDuration == Duration.Zero,
                IsUpdate = options.IsUpdate
            };

            if (options.IsUpdate)
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
                    setting.DelayDuration,
                    false, ct);
            }
        }

        private async Task TryWakeupAsync(UserNotification notification, MobilePushToken token,
            CancellationToken ct)
        {
            var nextWakeup = token.GetNextWakeupTime(clock);

            if (nextWakeup == null)
            {
                return;
            }

            var wakeupJob = new MobilePushJob(new UserNotification
            {
                AppId = notification.AppId,
                UserId = notification.UserId,
                UserLanguage = notification.UserLanguage
            }, token.Token, token.DeviceType)
            {
                IsImmediate = true,
                IsUpdate = false
            };

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
                log.LogWarning(ex, w => w
                    .WriteProperty("action", "UpdateMobileWakeupTime")
                    .WriteProperty("status", "Failed"));
            }
        }

        public async Task<bool> HandleAsync(MobilePushJob job, bool isLastAttempt,
            CancellationToken ct)
        {
            var id = job.Notification.Id;

            // If the notification is not scheduled it is very unlikey it has been confirmed already.
            if (!job.IsImmediate && await userNotificationStore.IsConfirmedOrHandledAsync(id, job.DeviceToken, Name, ct))
            {
                await UpdateAsync(job, ProcessStatus.Skipped);
            }
            else
            {
                await SendAsync(job, ct);
            }

            return true;
        }

        public Task HandleExceptionAsync(MobilePushJob job, Exception ex)
        {
            return UpdateAsync(job, ProcessStatus.Failed);
        }

        public async Task SendAsync(MobilePushJob job,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("SendMobilePush"))
            {
                var notification = job.Notification;

                var app = await appStore.GetCachedAsync(notification.AppId, ct);

                if (app == null)
                {
                    log.LogWarning(w => w
                        .WriteProperty("action", "SendMobilePush")
                        .WriteProperty("status", "Failed")
                        .WriteProperty("reason", "App not found"));

                    await UpdateAsync(job, ProcessStatus.Handled);
                    return;
                }

                try
                {
                    await UpdateAsync(job, ProcessStatus.Attempt);

                    var senders = integrationManager.Resolve<IMobilePushSender>(app, notification.Test).Select(x => x.Target).ToList();

                    if (senders.Count == 0)
                    {
                        await SkipAsync(job, Texts.Sms_ConfigReset);
                        return;
                    }

                    await SendCoreAsync(job, notification, app, senders, ct);

                    await UpdateAsync(job, ProcessStatus.Handled);
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(app.Id, Name, ex.Message);
                    throw;
                }
            }
        }

        private async Task SendCoreAsync(MobilePushJob job, UserNotification notification, App app, List<IMobilePushSender> senders,
            CancellationToken ct)
        {
            var lastSender = senders[^1];

            foreach (var sender in senders)
            {
                try
                {
                    var options = new MobilePushOptions
                    {
                        DeviceType = job.DeviceType,
                        DeviceToken = job.DeviceToken,
                        Wakeup = notification.Formatting == null
                    };

                    await sender.SendAsync(notification, options, ct);
                    return;
                }
                catch (MobilePushTokenExpiredException)
                {
                    await logStore.LogAsync(app.Id, Name, Texts.MobilePush_TokenRemoved);

                    var command = new RemoveUserMobileToken
                    {
                        Token = job.DeviceToken
                    };

                    await userStore.UpsertAsync(app.Id, notification.UserId, command, ct);
                    break;
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(app.Id, Name, ex.Message);

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
                await userNotificationStore.CollectAndUpdateAsync(job.Notification, Name, job.DeviceToken, status, reason);
            }
        }

        private async Task SkipAsync(MobilePushJob job, string reason)
        {
            await logStore.LogAsync(job.Notification.AppId, Name, reason);

            await UpdateAsync(job, ProcessStatus.Skipped);
        }
    }
}
