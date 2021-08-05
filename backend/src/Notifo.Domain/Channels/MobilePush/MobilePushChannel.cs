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
using Notifo.Domain.Utils;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Scheduling;
using Squidex.Hosting;
using Squidex.Log;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.MobilePush.MobilePushJob>;

namespace Notifo.Domain.Channels.MobilePush
{
    public sealed class MobilePushChannel : ICommunicationChannel, IScheduleHandler<MobilePushJob>, IInitializable
    {
        private readonly IAppStore appStore;
        private readonly IIntegrationManager integrationManager;
        private readonly ILogStore logStore;
        private readonly ISemanticLog log;
        private readonly IUserNotificationQueue userNotificationQueue;
        private readonly IUserNotificationStore userNotificationStore;
        private readonly IUserStore userStore;
        private readonly IClock clock;

        public int Order => 1000;

        public string Name => Providers.MobilePush;

        string ISystem.Name => $"Providers({Providers.MobilePush})";

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

        public Task InitializeAsync(CancellationToken ct)
        {
            userNotificationQueue.Subscribe(this);

            return Task.CompletedTask;
        }

        public IEnumerable<string> GetConfigurations(UserNotification notification, NotificationSetting settings, SendOptions options)
        {
            if (!integrationManager.IsConfigured<IMobilePushSender>(options.App, notification.Test))
            {
                yield break;
            }

            foreach (var token in options.User.MobilePushTokens)
            {
                yield return token.Token;
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
                IsImmediate = options.IsUpdate || setting.DelayDuration == Duration.Zero
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
                IsImmediate = true
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
            if (!job.IsImmediate && await userNotificationStore.IsConfirmedOrHandledAsync(job.Notification.Id, job.DeviceToken, Name))
            {
                await UpdateAsync(job.Notification, job.DeviceToken, ProcessStatus.Skipped);
            }
            else
            {
                await SendAsync(job, ct);
            }

            return true;
        }

        public Task HandleExceptionAsync(MobilePushJob job, Exception ex)
        {
            return UpdateAsync(job.Notification, job.DeviceToken, ProcessStatus.Failed);
        }

        public Task SendAsync(MobilePushJob job,
            CancellationToken ct)
        {
            return log.ProfileAsync("SendMobilePush", async () =>
            {
                var notification = job.Notification;

                var app = await appStore.GetCachedAsync(notification.AppId, ct);

                if (app == null)
                {
                    log.LogWarning(w => w
                        .WriteProperty("action", "SendMobilePush")
                        .WriteProperty("status", "Failed")
                        .WriteProperty("reason", "App not found"));

                    await UpdateAsync(notification, job.DeviceToken, ProcessStatus.Handled);
                    return;
                }

                try
                {
                    await UpdateAsync(notification, job.DeviceToken, ProcessStatus.Attempt);

                    var senders = integrationManager.Resolve<IMobilePushSender>(app, notification.Test).ToList();

                    if (senders.Count == 0)
                    {
                        await SkipAsync(notification, job.DeviceToken, Texts.Sms_ConfigReset, ct);
                        return;
                    }

                    await SendCoreAsync(job, notification, app, senders, ct);

                    await UpdateAsync(notification, job.DeviceToken, ProcessStatus.Handled);
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(app.Id, ex.Message, ct);
                    throw;
                }
            });
        }

        private async Task SendCoreAsync(MobilePushJob job, UserNotification notification, App app, List<IMobilePushSender> senders,
            CancellationToken ct)
        {
            var lastSender = senders.Last();

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
                    await logStore.LogAsync(app.Id, Texts.MobilePush_TokenRemoved, ct);

                    var command = new RemoveUserMobileToken
                    {
                        Token = job.DeviceToken
                    };

                    await userStore.UpsertAsync(app.Id, notification.UserId, command, ct);
                    break;
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(app.Id, ex.Message, ct);

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

        private Task UpdateAsync(UserNotification notification, string token, ProcessStatus status, string? reason = null)
        {
            return userNotificationStore.CollectAndUpdateAsync(notification, Name, token, status, reason);
        }

        private async Task SkipAsync(UserNotification notification, string token, string reason,
            CancellationToken ct)
        {
            await logStore.LogAsync(notification.AppId, reason, ct);

            await UpdateAsync(notification, token, ProcessStatus.Skipped);
        }
    }
}
