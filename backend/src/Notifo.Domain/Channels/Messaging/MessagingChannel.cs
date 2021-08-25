// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Scheduling;
using Squidex.Hosting;
using Squidex.Log;
using IMessagingTemplateStore = Notifo.Domain.ChannelTemplates.IChannelTemplateStore<Notifo.Domain.Channels.Messaging.MessagingTemplate>;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.Messaging.MessagingJob>;

namespace Notifo.Domain.Channels.Messaging
{
    public sealed class MessagingChannel : ICommunicationChannel, IScheduleHandler<MessagingJob>, IInitializable
    {
        private const string DefaultToken = "Default";
        private readonly IAppStore appStore;
        private readonly IIntegrationManager integrationManager;
        private readonly IMessagingFormatter messagingFormatter;
        private readonly IMessagingTemplateStore messagingTemplateStore;
        private readonly IUserNotificationStore userNotificationStore;
        private readonly IUserNotificationQueue userNotificationQueue;
        private readonly ILogStore logStore;
        private readonly ISemanticLog log;

        public int Order => 1000;

        public string Name => Providers.Messaging;

        string ISystem.Name => $"Providers({Providers.Messaging})";

        public MessagingChannel(ISemanticLog log, ILogStore logStore,
            IAppStore appStore,
            IIntegrationManager integrationManager,
            IMessagingFormatter messagingFormatter,
            IMessagingTemplateStore messagingTemplateStore,
            IUserNotificationQueue userNotificationQueue,
            IUserNotificationStore userNotificationStore)
        {
            this.appStore = appStore;
            this.log = log;
            this.logStore = logStore;
            this.integrationManager = integrationManager;
            this.messagingFormatter = messagingFormatter;
            this.messagingTemplateStore = messagingTemplateStore;
            this.userNotificationQueue = userNotificationQueue;
            this.userNotificationStore = userNotificationStore;
        }

        public Task InitializeAsync(CancellationToken ct)
        {
            userNotificationQueue.Subscribe(this);

            return Task.CompletedTask;
        }

        public IEnumerable<string> GetConfigurations(UserNotification notification, NotificationSetting settings, SendOptions options)
        {
            var senders = integrationManager.Resolve<IMessagingSender>(options.App, notification.Test);

            // Targets are email-addresses or phone-numbers or anything else to identify an user.
            if (senders.Any(x => x.Target.HasTarget(options.User)))
            {
                yield return DefaultToken;
            }
        }

        public async Task SendAsync(UserNotification notification, NotificationSetting setting, string configuration, SendOptions options,
            CancellationToken ct = default)
        {
            if (options.IsUpdate)
            {
                return;
            }

            var job = new MessagingJob(notification, setting.Template)
            {
                IsImmediate = setting.DelayDuration == Duration.Zero
            };

            var integrations = integrationManager.Resolve<IMessagingSender>(options.App, notification.Test);

            foreach (var (_, sender) in integrations)
            {
                await sender.AddTargetsAsync(job, options.User);
            }

            // Should not happen because we check before if there is at least one target.
            if (job.Targets.Count == 0)
            {
                await UpdateAsync(notification, ProcessStatus.Skipped);
            }

            await userNotificationQueue.ScheduleAsync(
                job.ScheduleKey,
                job,
                setting.DelayDuration,
                false, ct);
        }

        public Task HandleExceptionAsync(MessagingJob job, Exception ex)
        {
            return UpdateAsync(job.Notification, ProcessStatus.Failed);
        }

        public async Task<bool> HandleAsync(MessagingJob job, bool isLastAttempt,
            CancellationToken ct)
        {
            var id = job.Notification.Id;

            // If the notification is not scheduled it is very unlikey it has been confirmed already.
            if (!job.IsImmediate && await userNotificationStore.IsConfirmedOrHandledAsync(id, Name, DefaultToken, ct))
            {
                await UpdateAsync(job.Notification, ProcessStatus.Skipped);
            }
            else
            {
                await SendAsync(job, ct);
            }

            return false;
        }

        private async Task SendAsync(MessagingJob job,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("SendMessage"))
            {
                var app = await appStore.GetCachedAsync(job.Notification.AppId, ct);

                if (app == null)
                {
                    log.LogWarning(w => w
                        .WriteProperty("action", "SendMobilePush")
                        .WriteProperty("status", "Failed")
                        .WriteProperty("reason", "App not found"));

                    await UpdateAsync(job.Notification, ProcessStatus.Handled);
                    return;
                }

                try
                {
                    await UpdateAsync(job.Notification, ProcessStatus.Attempt);

                    var senders = integrationManager.Resolve<IMessagingSender>(app, job.Notification.Test).Select(x => x.Target).ToList();

                    if (senders.Count == 0)
                    {
                        await SkipAsync(job, Texts.Messaging_ConfigReset);
                        return;
                    }

                    var (skip, template) = await GetTemplateAsync(
                        job.Notification.AppId,
                        job.Notification.UserId,
                        job.TemplateName,
                        ct);

                    if (skip != null)
                    {
                        await SkipAsync(job, skip);
                        return;
                    }

                    var text = messagingFormatter.Format(template, job.Notification);

                    await SendCoreAsync(job, text, senders, ct);
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(job.Notification.AppId, Name, ex.Message);
                    throw;
                }
            }
        }

        private async Task SendCoreAsync(MessagingJob job, string text, List<IMessagingSender> senders,
            CancellationToken ct)
        {
            var lastSender = senders.Last();

            foreach (var sender in senders)
            {
                try
                {
                    var result = await sender.SendAsync(job, text, ct);

                    if (result)
                    {
                        await UpdateAsync(job.Notification, ProcessStatus.Handled);
                        return;
                    }
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(job.Notification.AppId, Name, ex.Message);

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

        private Task UpdateAsync(IUserNotification notification, ProcessStatus status, string? reason = null)
        {
            return userNotificationStore.CollectAndUpdateAsync(notification, Name, DefaultToken, status, reason);
        }

        private async Task SkipAsync(MessagingJob job, string reason)
        {
            await logStore.LogAsync(job.Notification.AppId, Name, reason);

            await UpdateAsync(job.Notification, ProcessStatus.Skipped);
        }

        private async Task<(string? Skip, MessagingTemplate?)> GetTemplateAsync(
            string appId,
            string language,
            string? name,
            CancellationToken ct)
        {
            var (status, template) = await messagingTemplateStore.GetBestAsync(appId, name, language, ct);

            switch (status)
            {
                case TemplateResolveStatus.ResolvedWithFallback:
                    {
                        var error = string.Format(CultureInfo.InvariantCulture, Texts.ChannelTemplate_ResolvedWithFallback, name);

                        await logStore.LogAsync(appId, Name, error);
                        break;
                    }

                case TemplateResolveStatus.NotFound when !string.IsNullOrWhiteSpace(name):
                    {
                        var error = string.Format(CultureInfo.InvariantCulture, Texts.ChannelTemplate_NotFound, name);

                        return (error, null);
                    }

                case TemplateResolveStatus.LanguageNotFound:
                    {
                        var error = string.Format(CultureInfo.InvariantCulture, Texts.ChannelTemplate_LanguageNotFound, language, name);

                        return (error, null);
                    }
            }

            return (null, template);
        }
    }
}
