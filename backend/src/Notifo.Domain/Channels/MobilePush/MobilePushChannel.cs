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
using FirebaseAdmin.Messaging;
using NodaTime;
using Notifo.Domain.Apps;
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
        private readonly FirebaseClientPool pool = new FirebaseClientPool();
        private readonly IAppStore appStore;
        private readonly ILogStore logStore;
        private readonly ISemanticLog log;
        private readonly IUserNotificationQueue userNotificationQueue;
        private readonly IUserNotificationStore userNotificationStore;
        private readonly IUserStore userStore;

        public int Order => 1000;

        public string Name => Providers.MobilePush;

        string ISystem.Name => $"Providers({Providers.MobilePush})";

        public MobilePushChannel(ISemanticLog log, ILogStore logStore,
            IAppStore appStore,
            IUserNotificationQueue userNotificationQueue,
            IUserNotificationStore userNotificationStore,
            IUserStore userStore)
        {
            this.appStore = appStore;
            this.log = log;
            this.logStore = logStore;
            this.userNotificationQueue = userNotificationQueue;
            this.userNotificationStore = userNotificationStore;
            this.userStore = userStore;
        }

        public Task InitializeAsync(CancellationToken ct)
        {
            userNotificationQueue.Subscribe(this);

            return Task.CompletedTask;
        }

        public bool CanSend(UserNotification notification, NotificationSetting setting, User user, App app)
        {
            return user.MobilePushTokens?.Count > 0 && IsFirebaseConfigured(app);
        }

        public async Task HandleSeenAsync(Guid id, SeenOptions options)
        {
            // Confirm the notification.
            userNotificationQueue.Complete(MobilePushJob.ComputeScheduleKey(id));

            if (!options.IsOffline || options.Channel != Name)
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

            // Send the notification to iOS devices only, because Android has a full queue.
            var tokens = user.MobilePushTokens.Where(x => x.DeviceType == MobileDeviceType.iOS).Select(x => x.Token).ToHashSet();

            if (tokens.Count == 0)
            {
                return;
            }

            // Send an silent notification to the user.
            var job = new MobilePushJob(new UserNotification
            {
                Id = notification.Id,
                AppId = notification.AppId,
                UserId = notification.UserId,
                UserLanguage = notification.UserLanguage
            }, tokens);

            await userNotificationQueue.ScheduleAsync(
                job.ScheduleKey,
                job,
                default(Instant),
                false);
        }

        public Task SendAsync(UserNotification notification, NotificationSetting setting, User user, App app, SendOptions options, CancellationToken ct)
        {
            var tokens = user.MobilePushTokens.Select(x => x.Token).ToHashSet();

            if (tokens.Count == 0)
            {
                return Task.CompletedTask;
            }

            var job = new MobilePushJob(notification, tokens);

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

        public async Task<bool> HandleAsync(List<MobilePushJob> jobs, bool isLastAttempt, CancellationToken ct)
        {
            // We are not using grouped scheduling here.
            var job = jobs[0];

            if (await userNotificationStore.IsConfirmed(job.Notification.Id, Name))
            {
                await UpdateAsync(job.Notification, ProcessStatus.Skipped);
            }
            else
            {
                await SendAsync(job, isLastAttempt, ct);
            }

            // Remove the job from the queue when it is a normal notification.
            return job.Notification != null;
        }

        public Task SendAsync(MobilePushJob job, bool isLastAttempt, CancellationToken ct)
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

                    return;
                }

                try
                {
                    await UpdateAsync(notification, ProcessStatus.Attempt);

                    if (!IsFirebaseConfigured(app))
                    {
                        throw new DomainException(Texts.MobilePush_ConfigReset);
                    }

                    var messaging = pool.GetMessaging(app);

                    await SendAnyAsync(job, messaging, ct);

                    await UpdateAsync(notification, ProcessStatus.Handled);
                }
                catch (Exception ex)
                {
                    if (isLastAttempt)
                    {
                        await UpdateAsync(notification, ProcessStatus.Failed);
                    }

                    if (ex is DomainException domainException)
                    {
                        await logStore.LogAsync(app.Id, domainException.Message);
                    }
                    else
                    {
                        throw;
                    }
                }
            });
        }

        private async Task SendAnyAsync(MobilePushJob job, FirebaseMessaging messaging, CancellationToken ct)
        {
            var tasks = job.Tokens.Select(token => SendAsync(job.Notification, messaging, token, ct)).ToArray();
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
                        w.WriteProperty("action", "SendMobilePush");
                        w.WriteProperty("status", "FailedPartially");

                        Profiler.Session?.Write(w);
                    });
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task SendAsync(UserNotification notification, FirebaseMessaging messaging, string token, CancellationToken ct)
        {
            try
            {
                var message = notification.ToFirebaseMessage(token, notification.Formatting == null);

                await messaging.SendAsync(message, ct);
            }
            catch (FirebaseMessagingException ex) when (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                if (notification != null)
                {
                    await logStore.LogAsync(notification.AppId, Texts.MobilePush_TokenRemoved, ct);

                    var command = new RemoveUserMobileToken
                    {
                        Token = token
                    };

                    await userStore.UpsertAsync(notification.AppId, notification.UserId, command, ct);
                }
            }
            catch (FirebaseMessagingException ex)
            {
                await logStore.LogAsync(notification.AppId, ex.Message, ct);
            }
        }

        private static bool IsFirebaseConfigured(App app)
        {
            return !string.IsNullOrWhiteSpace(app.FirebaseProject) || !string.IsNullOrWhiteSpace(app.FirebaseCredential);
        }

        private Task UpdateAsync(UserNotification notification, ProcessStatus status, string? reason = null)
        {
            return userNotificationStore.CollectAndUpdateAsync(notification, Name, status, reason);
        }
    }
}
