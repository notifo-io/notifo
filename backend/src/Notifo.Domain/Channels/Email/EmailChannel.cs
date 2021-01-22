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
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.Email.EmailJob>;

namespace Notifo.Domain.Channels.Email
{
    public sealed class EmailChannel : IInitializable, ICommunicationChannel, IScheduleHandler<EmailJob>
    {
        private readonly IAppStore appStore;
        private readonly IEmailFormatter emailFormatter;
        private readonly IEmailServer emailServer;
        private readonly ILogStore logStore;
        private readonly ISemanticLog log;
        private readonly IUserNotificationQueue userNotificationQueue;
        private readonly IUserNotificationStore userNotificationStore;
        private readonly IUserStore userStore;

        public int Order => 1000;

        public string Name => Providers.Email;

        string ISystem.Name => $"Providers({Providers.Email})";

        public EmailChannel(ISemanticLog log, ILogStore logStore,
            IAppStore appStore,
            IEmailFormatter emailFormatter,
            IEmailServer emailServer,
            IUserNotificationQueue userNotificationQueue,
            IUserNotificationStore userNotificationStore,
            IUserStore userStore)
        {
            this.appStore = appStore;
            this.emailFormatter = emailFormatter;
            this.emailServer = emailServer;
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

        public bool CanSend(UserNotification notification, NotificationSetting preference, User user, App app)
        {
            if (!app.AllowEmail)
            {
                return false;
            }

            if (notification.Silent || string.IsNullOrEmpty(user.EmailAddress) || app.EmailVerificationStatus != EmailVerificationStatus.Verified)
            {
                return false;
            }

            if (!app.EmailTemplates.ContainsKey(notification.UserLanguage))
            {
                return false;
            }

            return true;
        }

        public Task SendAsync(UserNotification notification, NotificationSetting setting, User user, App app, bool isUpdate, CancellationToken ct)
        {
            if (isUpdate)
            {
                return Task.CompletedTask;
            }

            var job = new EmailJob(notification);

            return userNotificationQueue.ScheduleDelayedAsync(
                job.ScheduleKey,
                job,
                setting.DelayInSecondsOrZero,
                false, ct);
        }

        public async Task HandleAsync(List<EmailJob> jobs, bool isLastAttempt, CancellationToken ct)
        {
            var notifications = new List<UserNotification>();

            foreach (var job in jobs)
            {
                if (await userNotificationStore.IsConfirmedOrHandled(job.Notification.Id, Name))
                {
                    await UpdateAsync(notifications, ProcessStatus.Skipped);
                }
                else
                {
                    notifications.Add(job.Notification);
                }
            }

            if (notifications.Any())
            {
                await SendAsync(notifications, isLastAttempt, ct);
            }
        }

        public Task SendAsync(List<UserNotification> notifications, bool isLastAttempt, CancellationToken ct)
        {
            return log.ProfileAsync("SendEmail", async () =>
            {
                await UpdateAsync(notifications, ProcessStatus.Attempt);

                var first = notifications[0];

                var app = await appStore.GetCachedAsync(first.AppId, ct);

                if (app == null)
                {
                    log.LogWarning(w => w
                        .WriteProperty("action", "SendEmail")
                        .WriteProperty("status", "Failed")
                        .WriteProperty("reason", "App not found"));

                    return;
                }

                try
                {
                    var user = await userStore.GetCachedAsync(first.AppId, first.UserId, ct);

                    if (user == null)
                    {
                        throw new DomainException(Texts.Email_UserNoEmail);
                    }

                    if (string.IsNullOrWhiteSpace(user.EmailAddress))
                    {
                        throw new DomainException(Texts.Email_UserNoEmail);
                    }

                    await SendAsync(notifications, app, user, ct);

                    await UpdateAsync(notifications, ProcessStatus.Handled);
                }
                catch (Exception ex)
                {
                    if (isLastAttempt)
                    {
                        await UpdateAsync(notifications, ProcessStatus.Failed);
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

        private async Task SendAsync(List<UserNotification> notifications, App app, User user, CancellationToken ct)
        {
            var message = await emailFormatter.FormatAsync(notifications, app, user);

            await emailServer.SendAsync(message, ct);
        }

        private async Task UpdateAsync(List<UserNotification> notifications, ProcessStatus status, string? reason = null)
        {
            foreach (var notification in notifications)
            {
                await UpdateAsync(notification, status, reason);
            }
        }

        private async Task UpdateAsync(UserNotification notification, ProcessStatus status, string? reason = null)
        {
            await userNotificationStore.CollectAndUpdateAsync(notification, Name, status, reason);
        }
    }
}
