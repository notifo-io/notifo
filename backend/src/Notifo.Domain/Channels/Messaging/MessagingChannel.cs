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
using Notifo.Domain.Utils;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Scheduling;
using Squidex.Hosting;
using Squidex.Log;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.Messaging.MessagingJob>;

namespace Notifo.Domain.Channels.Messaging
{
    public sealed class MessagingChannel : ICommunicationChannel, IScheduleHandler<MessagingJob>, IInitializable
    {
        private const string DefaultToken = "Default";
        private readonly ILogStore logStore;
        private readonly IAppStore appStore;
        private readonly IIntegrationManager integrationManager;
        private readonly IUserNotificationStore userNotificationStore;
        private readonly IUserNotificationQueue userNotificationQueue;
        private readonly ISemanticLog log;

        public int Order => 1000;

        public string Name => Providers.Messaging;

        string ISystem.Name => $"Providers({Providers.Messaging})";

        public MessagingChannel(ISemanticLog log, ILogStore logStore,
            IAppStore appStore,
            IIntegrationManager integrationManager,
            IUserNotificationQueue userNotificationQueue,
            IUserNotificationStore userNotificationStore)
        {
            this.appStore = appStore;
            this.log = log;
            this.logStore = logStore;
            this.integrationManager = integrationManager;
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
            var senders = integrationManager.Resolve<IMessagingSender>(options.App);

            // Targets are email-addresses or phone-numbers or anything else to identify an user.
            if (senders.Any(x => x.HasTarget(options.User)))
            {
                yield return DefaultToken;
            }
        }

        public async Task SendAsync(UserNotification notification, NotificationSetting setting, string configuration, SendOptions options, CancellationToken ct = default)
        {
            if (options.IsUpdate)
            {
                return;
            }

            var job = new MessagingJob(notification)
            {
                IsImmediate = setting.DelayDuration == Duration.Zero
            };

            var senders = integrationManager.Resolve<IMessagingSender>(options.App);

            foreach (var sender in senders)
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

        public async Task<bool> HandleAsync(MessagingJob job, bool isLastAttempt, CancellationToken ct)
        {
            if (!job.IsImmediate && await userNotificationStore.IsConfirmedOrHandled(job.Notification.Id, Name, DefaultToken))
            {
                await UpdateAsync(job.Notification, ProcessStatus.Skipped);
            }
            else
            {
                await SendAsync(job, ct);
            }

            return false;
        }

        private Task SendAsync(MessagingJob job, CancellationToken ct)
        {
            return log.ProfileAsync("SendMessaging", async () =>
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

                    var senders = integrationManager.Resolve<IMessagingSender>(app).ToList();

                    if (senders.Count == 0)
                    {
                        await SkipAsync(job, Texts.Messaging_ConfigReset, ct);
                        return;
                    }

                    await SendCoreAsync(job, senders, ct);
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(job.Notification.AppId, ex.Message, ct);
                    throw;
                }
            });
        }

        private async Task SendCoreAsync(MessagingJob job, List<IMessagingSender> senders, CancellationToken ct)
        {
            var lastSender = senders.Last();

            foreach (var sender in senders)
            {
                try
                {
                    var result = await sender.SendAsync(job, ct);

                    if (result)
                    {
                        await UpdateAsync(job.Notification, ProcessStatus.Handled);
                        return;
                    }
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(job.Notification.AppId, ex.Message, ct);

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

        private async Task SkipAsync(MessagingJob job, string reason, CancellationToken ct)
        {
            await logStore.LogAsync(job.Notification.AppId, reason, ct);

            await UpdateAsync(job.Notification, ProcessStatus.Skipped);
        }
    }
}
