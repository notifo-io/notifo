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
using IEmailTemplateStore = Notifo.Domain.ChannelTemplates.IChannelTemplateStore<Notifo.Domain.Channels.Email.EmailTemplate>;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.Email.EmailJob>;

namespace Notifo.Domain.Channels.Email
{
    public sealed class EmailChannel : IInitializable, ICommunicationChannel, IScheduleHandler<EmailJob>
    {
        private readonly IAppStore appStore;
        private readonly IIntegrationManager integrationManager;
        private readonly IEmailFormatter emailFormatter;
        private readonly IEmailTemplateStore emailTemplateStore;
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
            IIntegrationManager integrationManager,
            IEmailFormatter emailFormatter,
            IEmailTemplateStore emailTemplateStore,
            IUserNotificationQueue userNotificationQueue,
            IUserNotificationStore userNotificationStore,
            IUserStore userStore)
        {
            this.appStore = appStore;
            this.emailFormatter = emailFormatter;
            this.emailTemplateStore = emailTemplateStore;
            this.log = log;
            this.logStore = logStore;
            this.integrationManager = integrationManager;
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
            if (!integrationManager.IsConfigured<IEmailSender>(options.App, notification.Test))
            {
                yield break;
            }

            if (notification.Silent || string.IsNullOrEmpty(options.User.EmailAddress))
            {
                yield break;
            }

            yield return options.User.EmailAddress;
        }

        public Task SendAsync(UserNotification notification, NotificationSetting setting, string configuration, SendOptions options,
            CancellationToken ct)
        {
            if (options.IsUpdate)
            {
                return Task.CompletedTask;
            }

            var job = new EmailJob(notification, setting.Template, configuration);

            return userNotificationQueue.ScheduleGroupedAsync(
                job.ScheduleKey,
                job,
                setting.DelayDuration,
                false, ct);
        }

        public async Task<bool> HandleAsync(List<EmailJob> jobs, bool isLastAttempt,
            CancellationToken ct)
        {
            var nonConfirmed = new List<EmailJob>();

            foreach (var job in jobs)
            {
                if (await userNotificationStore.IsConfirmedOrHandledAsync(job.Notification.Id, job.EmailAddress, Name, ct))
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

        public Task HandleExceptionAsync(List<EmailJob> jobs, Exception ex)
        {
            return UpdateAsync(jobs, jobs[0].EmailAddress, ProcessStatus.Failed);
        }

        public async Task SendAsync(List<EmailJob> jobs,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("SendEmail"))
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
                        await SkipAsync(jobs, commonEmail, Texts.Email_UserDeleted);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(user.EmailAddress))
                    {
                        await SkipAsync(jobs, commonEmail, Texts.Email_UserNoEmail);
                        return;
                    }

                    var senders = integrationManager.Resolve<IEmailSender>(app, first.Notification.Test).Select(x => x.Target).ToList();

                    if (senders.Count == 0)
                    {
                        await SkipAsync(jobs, commonEmail, Texts.Email_ConfigReset);
                        return;
                    }

                    EmailMessage? message;

                    using (Telemetry.Activities.StartActivity("Format"))
                    {
                        var template = await emailTemplateStore.GetBestAsync(
                            first.Notification.AppId,
                            first.EmailTemplate,
                            first.Notification.UserLanguage,
                            ct);

                        if (template == null)
                        {
                            await SkipAsync(jobs, commonEmail, Texts.Email_TemplateNotFound);
                            return;
                        }

                        message = await emailFormatter.FormatAsync(jobs.Select(x => x.Notification), template, app, user, ct);
                    }

                    await SendCoreAsync(message, app.Id, senders, ct);

                    await UpdateAsync(jobs, commonEmail, ProcessStatus.Handled);
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(app.Id, ex.Message, ct);
                    throw;
                }
            }
        }

        private async Task SendCoreAsync(EmailMessage message, string appId, List<IEmailSender> senders,
            CancellationToken ct)
        {
            var lastSender = senders.Last();

            foreach (var sender in senders)
            {
                try
                {
                    await sender.SendAsync(message, ct);
                    return;
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(appId, ex.Message, ct);

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

        private Task UpdateAsync(IUserNotification notification, string email, ProcessStatus status, string? reason = null)
        {
            return userNotificationStore.CollectAndUpdateAsync(notification, Name, email, status, reason);
        }

        private async Task SkipAsync(List<EmailJob> jobs, string email, string reason)
        {
            await logStore.LogAsync(jobs[0].Notification.AppId, reason);

            await UpdateAsync(jobs, email, ProcessStatus.Skipped);
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
