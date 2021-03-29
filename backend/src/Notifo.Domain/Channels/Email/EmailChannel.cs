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

        public IEnumerable<string> GetConfigurations(UserNotification notification, NotificationSetting settings, SendOptions options)
        {
            if (!options.App.AllowEmail)
            {
                yield break;
            }

            if (notification.Silent || string.IsNullOrEmpty(options.User.EmailAddress) || options.App.EmailVerificationStatus != EmailVerificationStatus.Verified)
            {
                yield break;
            }

            if (!options.App.EmailTemplates.ContainsKey(notification.UserLanguage))
            {
                yield break;
            }

            yield return options.User.EmailAddress;
        }

        public Task SendAsync(UserNotification notification, NotificationSetting setting, string configuration, SendOptions options, CancellationToken ct)
        {
            if (options.IsUpdate)
            {
                return Task.CompletedTask;
            }

            var job = new EmailJob(notification, configuration);

            return userNotificationQueue.ScheduleGroupedAsync(
                job.ScheduleKey,
                job,
                setting.DelayDuration,
                false, ct);
        }

        public async Task<bool> HandleAsync(List<EmailJob> jobs, bool isLastAttempt, CancellationToken ct)
        {
            var nonConfirmed = new List<EmailJob>();

            foreach (var job in jobs)
            {
                if (await userNotificationStore.IsConfirmedOrHandled(job.Notification.Id, job.EmailAddress, Name))
                {
                    await UpdateAsync(job.Notification, job.EmailAddress, ProcessStatus.Skipped);
                }
                else
                {
                    nonConfirmed.Add(job);
                }
            }

            if (nonConfirmed.Any())
            {
                await SendAsync(nonConfirmed, ct);
            }

            return true;
        }

        public Task SendAsync(List<EmailJob> jobs, CancellationToken ct)
        {
            return log.ProfileAsync("SendEmail", async () =>
            {
                var first = jobs[0];

                var commonEmail = first.EmailAddress;
                var commonApp = first.Notification.AppId;
                var commonUser = first.Notification.UserId;

                await UpdateAsync(first.Notification, commonEmail, ProcessStatus.Attempt);

                var app = await appStore.GetCachedAsync(first.Notification.AppId, ct);

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
                    var user = await userStore.GetCachedAsync(commonApp, commonUser, ct);

                    if (user == null)
                    {
                        throw new DomainException(Texts.Email_UserNoEmail);
                    }

                    if (string.IsNullOrWhiteSpace(user.EmailAddress))
                    {
                        throw new DomainException(Texts.Email_UserNoEmail);
                    }

                    await SendCoreAsync(jobs.Select(x => x.Notification), app, user, ct);

                    await UpdateAsync(jobs, commonEmail, ProcessStatus.Handled);
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(app.Id, ex.Message);
                    throw;
                }
            });
        }

        private async Task SendCoreAsync(IEnumerable<UserNotification> notifications, App app, User user, CancellationToken ct)
        {
            var message = await emailFormatter.FormatAsync(notifications, app, user);

            await emailServer.SendAsync(message, ct);
        }

        public Task HandleExceptionAsync(List<EmailJob> jobs, Exception ex)
        {
            return UpdateAsync(jobs, jobs[0].EmailAddress, ProcessStatus.Failed);
        }

        private Task UpdateAsync(UserNotification notification, string email, ProcessStatus status, string? reason = null)
        {
            return userNotificationStore.CollectAndUpdateAsync(notification, Name, email, status, reason);
        }

        private async Task UpdateAsync(List<EmailJob> jobs, string email, ProcessStatus status, string? reason = null)
        {
            foreach (var job in jobs)
            {
                await UpdateAsync(job.Notification, email, status, reason);
            }
        }
    }
}
