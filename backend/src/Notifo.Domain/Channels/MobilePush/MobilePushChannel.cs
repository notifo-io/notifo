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
using Notifo.Domain.Apps;
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Initialization;
using Notifo.Infrastructure.Scheduling;
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

        public int InitializationOrder => 1000;

        public string Name => Providers.MobilePush;

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

        public Task InitializeAsync(CancellationToken ct = default)
        {
            userNotificationQueue.Subscribe(this);

            return Task.CompletedTask;
        }

        public bool CanSend(UserNotification notification, NotificationSetting setting, User user, App app)
        {
            return !notification.Silent && user.MobilePushTokens?.Count > 0 && IsFirebaseConfigured(app);
        }

        public Task SendAsync(UserNotification notification, NotificationSetting setting, User user, App app, bool isUpdate, CancellationToken ct = default)
        {
            var job = new MobilePushJob(notification, user);

            return userNotificationQueue.ScheduleDelayedAsync(
                job.ScheduleKey,
                job,
                setting.DelayInSecondsOrZero,
                false, ct);
        }

        public async Task HandleAsync(List<MobilePushJob> jobs, bool isLastAttempt, CancellationToken ct)
        {
            foreach (var job in jobs)
            {
                if (await userNotificationStore.IsConfirmedOrHandled(job.Notification.Id, Name))
                {
                    await UpdateAsync(job.Notification, ProcessStatus.Skipped);
                }
                else
                {
                    await SendAsync(job, isLastAttempt, ct);
                }
            }
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
                var message = new Message
                {
                    Token = token,
                    Notification = new Notification
                    {
                        Title = notification.Formatting.Subject
                    }
                };

                if (!string.IsNullOrWhiteSpace(notification.Formatting.Body))
                {
                    message.Notification.Body = notification.Formatting.Body;
                }

                await messaging.SendAsync(message, ct);
            }
            catch (FirebaseMessagingException ex) when (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                await logStore.LogAsync(notification.AppId, Texts.MobilePush_TokenRemoved, ct);

                var command = new RemoveUserMobileToken
                {
                    Token = token
                };

                await userStore.UpsertAsync(notification.AppId, notification.UserId, command, ct);
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
