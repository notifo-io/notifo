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
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.Sms.SmsJob>;

namespace Notifo.Domain.Channels.Sms
{
    public sealed class SmsChannel : ICommunicationChannel, IScheduleHandler<SmsJob>, IInitializable
    {
        private readonly ILogStore logStore;
        private readonly IAppStore appStore;
        private readonly IIntegrationManager integrationManager;
        private readonly IUserNotificationStore userNotificationStore;
        private readonly IUserNotificationQueue userNotificationQueue;
        private readonly ISemanticLog log;

        public int Order => 1000;

        public string Name => Providers.Sms;

        string ISystem.Name => $"Providers({Providers.Sms})";

        public SmsChannel(ISemanticLog log, ILogStore logStore,
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
            if (!integrationManager.IsConfigured<ISmsSender>(options.App, notification.Test))
            {
                yield break;
            }

            if (!string.IsNullOrWhiteSpace(options.User.PhoneNumber))
            {
                yield return options.User.PhoneNumber;
            }
        }

        public async Task HandleResponseAsync(SmsResponse response)
        {
            if (Guid.TryParse(response.Reference, out var id))
            {
                var notification = await userNotificationStore.FindAsync(id);

                if (notification != null)
                {
                    await UpdateAsync(response, notification);
                }

                if (response.Status == SmsResult.Delivered)
                {
                    userNotificationQueue.Complete(SmsJob.ComputeScheduleKey(id));
                }
            }
        }

        private async Task UpdateAsync(SmsResponse response, UserNotification notification)
        {
            var phoneNumber = response.Recipient;

            if (!notification.Channels.TryGetValue(Name, out var channel))
            {
                return;
            }

            if (channel.Status.TryGetValue(response.Recipient, out var status) && status.Status == ProcessStatus.Attempt)
            {
                switch (response.Status)
                {
                    case SmsResult.Delivered:
                        await UpdateAsync(notification, phoneNumber, ProcessStatus.Handled);
                        break;
                    case SmsResult.Failed:
                        await UpdateAsync(notification, phoneNumber, ProcessStatus.Failed);
                        break;
                }
            }
        }

        public Task SendAsync(UserNotification notification, NotificationSetting setting, string configuration, SendOptions options, CancellationToken ct = default)
        {
            if (options.IsUpdate)
            {
                return Task.CompletedTask;
            }

            var job = new SmsJob(notification, configuration)
            {
                IsImmediate = setting.DelayDuration == Duration.Zero
            };

            return userNotificationQueue.ScheduleAsync(
                job.ScheduleKey,
                job,
                setting.DelayDuration,
                false, ct);
        }

        public Task HandleExceptionAsync(SmsJob job, Exception ex)
        {
            return UpdateAsync(job, job.PhoneNumber, ProcessStatus.Failed);
        }

        public async Task<bool> HandleAsync(SmsJob job, bool isLastAttempt, CancellationToken ct)
        {
            if (!job.IsImmediate && await userNotificationStore.IsConfirmedOrHandled(job.Id, Name, job.PhoneNumber))
            {
                await UpdateAsync(job, job.PhoneNumber, ProcessStatus.Skipped);
            }
            else
            {
                await SendAsync(job, ct);
            }

            return false;
        }

        private Task SendAsync(SmsJob job, CancellationToken ct)
        {
            return log.ProfileAsync("SendSms", async () =>
            {
                var app = await appStore.GetCachedAsync(job.AppId, ct);

                if (app == null)
                {
                    log.LogWarning(w => w
                        .WriteProperty("action", "SendMobilePush")
                        .WriteProperty("status", "Failed")
                        .WriteProperty("reason", "App not found"));

                    await UpdateAsync(job, job.PhoneNumber, ProcessStatus.Handled);
                    return;
                }

                try
                {
                    await UpdateAsync(job, job.PhoneNumber, ProcessStatus.Attempt);

                    var senders = integrationManager.Resolve<ISmsSender>(app, job.Test).ToList();

                    if (senders.Count == 0)
                    {
                        await SkipAsync(job, job.PhoneNumber, Texts.Sms_ConfigReset, ct);
                        return;
                    }

                    await SendCoreAsync(job, senders, ct);
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(job.AppId, ex.Message, ct);
                    throw;
                }
            });
        }

        private async Task SendCoreAsync(SmsJob job, List<ISmsSender> senders, CancellationToken ct)
        {
            var lastSender = senders.Last();

            foreach (var sender in senders)
            {
                await sender.RegisterAsync(HandleResponseAsync);
            }

            foreach (var sender in senders)
            {
                try
                {
                    var result = await sender.SendAsync(job.PhoneNumber, job.Text, job.Id.ToString(), ct);

                    if (result == SmsResult.Delivered)
                    {
                        await UpdateAsync(job, job.PhoneNumber, ProcessStatus.Handled);
                        return;
                    }
                    else if (result == SmsResult.Sent)
                    {
                        return;
                    }
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(job.AppId, ex.Message, ct);

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

        private Task UpdateAsync(IUserNotification token, string phoneNumber, ProcessStatus status, string? reason = null)
        {
            return userNotificationStore.CollectAndUpdateAsync(token, Name, phoneNumber, status, reason);
        }

        private async Task SkipAsync(SmsJob job, string token, string reason, CancellationToken ct)
        {
            await logStore.LogAsync(job.AppId, reason, ct);

            await UpdateAsync(job, token, ProcessStatus.Skipped);
        }
    }
}
