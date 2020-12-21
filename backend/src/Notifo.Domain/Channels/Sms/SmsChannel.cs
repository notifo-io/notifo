// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Apps;
using Notifo.Domain.Log;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Initialization;
using Notifo.Infrastructure.Scheduling;
using Squidex.Log;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.Sms.SmsJob>;

namespace Notifo.Domain.Channels.Sms
{
    public sealed class SmsChannel : ICommunicationChannel, IScheduleHandler<SmsJob>, IInitializable
    {
        private readonly ILogStore logStore;
        private readonly ISmsSender smsSender;
        private readonly IUserNotificationStore userNotificationStore;
        private readonly IUserNotificationQueue userNotificationQueue;
        private readonly ISemanticLog log;

        public int InitializationOrder => 1000;

        public string Name => Providers.Sms;

        public SmsChannel(ISemanticLog log, ILogStore logStore,
            ISmsSender smsSender,
            IUserNotificationQueue userNotificationQueue,
            IUserNotificationStore userNotificationStore)
        {
            this.log = log;
            this.logStore = logStore;
            this.smsSender = smsSender;
            this.userNotificationQueue = userNotificationQueue;
            this.userNotificationStore = userNotificationStore;
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            userNotificationQueue.Subscribe(this);

            smsSender.RegisterAsync(HandleResponseAsync);

            return Task.CompletedTask;
        }

        public bool CanSend(UserNotification notification, NotificationSetting setting, User user, App app)
        {
            if (!app.AllowSms)
            {
                return false;
            }

            return !notification.Silent && !string.IsNullOrWhiteSpace(user.PhoneNumber);
        }

        public async Task HandleResponseAsync(string reference, SmsResult result)
        {
            if (Guid.TryParse(reference, out var id))
            {
                var notification = await userNotificationStore.FindAsync(id);

                if (notification != null && notification.Sending.TryGetValue(Name, out var sending) && sending.Status == ProcessStatus.Attempt)
                {
                    if (result == SmsResult.Delivered)
                    {
                        await UpdateAsync(notification, ProcessStatus.Handled);
                    }
                    else if (result == SmsResult.Failed)
                    {
                        await UpdateAsync(notification, ProcessStatus.Handled);
                    }
                }
            }
        }

        public Task SendAsync(UserNotification notification, NotificationSetting setting, User user, App app, bool isUpdate, CancellationToken ct = default)
        {
            var job = new SmsJob(notification, user);

            return userNotificationQueue.ScheduleDelayedAsync(
                job.ScheduleKey,
                job,
                setting.DelayInSecondsOrZero,
                false, ct);
        }

        public async Task HandleAsync(List<SmsJob> jobs, bool isLastAttempt, CancellationToken ct)
        {
            foreach (var job in jobs)
            {
                if (await userNotificationStore.IsConfirmedOrHandled(job.Id, Name))
                {
                    await UpdateAsync(job, ProcessStatus.Skipped);
                }
                else
                {
                    await SendAsync(job, isLastAttempt, ct);
                }
            }
        }

        public Task SendAsync(SmsJob job, bool isLastAttempt, CancellationToken ct)
        {
            return log.ProfileAsync("SendSms", async () =>
            {
                try
                {
                    await UpdateAsync(job, ProcessStatus.Attempt);

                    var result = await smsSender.SendAsync(job.PhoneNumber, job.Text, job.Id.ToString(), ct);

                    if (result == SmsResult.Delivered)
                    {
                        await UpdateAsync(job, ProcessStatus.Handled);
                    }
                }
                catch (Exception ex)
                {
                    if (isLastAttempt)
                    {
                        await UpdateAsync(job, ProcessStatus.Failed);
                    }

                    if (ex is DomainException domainException)
                    {
                        await logStore.LogAsync(job.AppId, domainException.Message);
                    }
                    else
                    {
                        throw;
                    }
                }
            });
        }

        private Task UpdateAsync(IUserNotification token, ProcessStatus status, string? reason = null)
        {
            return userNotificationStore.CollectAndUpdateAsync(token, Name, status, reason);
        }
    }
}
