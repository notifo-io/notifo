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
using NodaTime;
using Notifo.Domain.Log;
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
        private readonly ISmsSender smsSender;
        private readonly IUserNotificationStore userNotificationStore;
        private readonly IUserNotificationQueue userNotificationQueue;
        private readonly ISemanticLog log;

        public int Order => 1000;

        public string Name => Providers.Sms;

        string ISystem.Name => $"Providers({Providers.Sms})";

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

        public Task InitializeAsync(CancellationToken ct)
        {
            userNotificationQueue.Subscribe(this);

            smsSender.RegisterAsync(HandleResponseAsync);

            return Task.CompletedTask;
        }

        public IEnumerable<string> GetConfigurations(UserNotification notification, NotificationSetting settings, SendOptions options)
        {
            if (!options.App.AllowSms || notification.Silent)
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

                var phoneNumber = response.Recipient;

                if (notification != null &&
                    notification.Channels.TryGetValue(Name, out var channel) &&
                    channel.Status.TryGetValue(response.Recipient, out var channelStatus) &&
                    channelStatus.Status == ProcessStatus.Attempt)
                {
                    switch (response.Status)
                    {
                        case SmsResult.Delivered:
                            await UpdateAsync(notification, phoneNumber, ProcessStatus.Handled);
                            break;
                        case SmsResult.Failed:
                            await UpdateAsync(notification, phoneNumber, ProcessStatus.Handled);
                            break;
                    }
                }

                // Confirm the job in the scheduler when delivered.
                if (response.Status == SmsResult.Delivered)
                {
                    userNotificationQueue.Complete(SmsJob.ComputeScheduleKey(id));
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

        public async Task<bool> HandleAsync(SmsJob job, bool isLastAttempt, CancellationToken ct)
        {
            if (!job.IsImmediate && await userNotificationStore.IsConfirmedOrHandled(job.Id, job.PhoneNumber, Name))
            {
                await UpdateAsync(job, job.PhoneNumber, ProcessStatus.Skipped);
            }
            else
            {
                await SendAsync(job, ct);
            }

            return false;
        }

        public Task SendAsync(SmsJob job, CancellationToken ct)
        {
            return log.ProfileAsync("SendSms", async () =>
            {
                try
                {
                    await UpdateAsync(job, job.PhoneNumber, ProcessStatus.Attempt);

                    var result = await smsSender.SendAsync(job.PhoneNumber, job.Text, job.Id.ToString(), ct);

                    if (result == SmsResult.Delivered)
                    {
                        await UpdateAsync(job, job.PhoneNumber, ProcessStatus.Handled);
                    }
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(job.AppId, ex.Message);
                    throw;
                }
            });
        }

        public Task HandleExceptionAsync(SmsJob job, Exception ex)
        {
            return UpdateAsync(job, job.PhoneNumber, ProcessStatus.Failed);
        }

        private Task UpdateAsync(IUserNotification token, string phoneNumber, ProcessStatus status, string? reason = null)
        {
            return userNotificationStore.CollectAndUpdateAsync(token, Name, phoneNumber, status, reason);
        }
    }
}
