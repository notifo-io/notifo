// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using System.Globalization;
using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Scheduling;
using Squidex.Log;
using ISmsTemplateStore = Notifo.Domain.ChannelTemplates.IChannelTemplateStore<Notifo.Domain.Channels.Sms.SmsTemplate>;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.Sms.SmsJob>;

namespace Notifo.Domain.Channels.Sms
{
    public sealed class SmsChannel : ICommunicationChannel, IScheduleHandler<SmsJob>, ISmsCallback
    {
        private readonly IAppStore appStore;
        private readonly IIntegrationManager integrationManager;
        private readonly ILogStore logStore;
        private readonly ISmsFormatter smsFormatter;
        private readonly ISmsTemplateStore smsTemplateStore;
        private readonly IUserNotificationQueue userNotificationQueue;
        private readonly IUserNotificationStore userNotificationStore;
        private readonly ISemanticLog log;

        public string Name => Providers.Sms;

        public SmsChannel(ISemanticLog log, ILogStore logStore,
            IAppStore appStore,
            IIntegrationManager integrationManager,
            ISmsFormatter smsFormatter,
            ISmsTemplateStore smsTemplateStore,
            IUserNotificationQueue userNotificationQueue,
            IUserNotificationStore userNotificationStore)
        {
            this.appStore = appStore;
            this.integrationManager = integrationManager;
            this.log = log;
            this.logStore = logStore;
            this.smsFormatter = smsFormatter;
            this.smsTemplateStore = smsTemplateStore;
            this.userNotificationQueue = userNotificationQueue;
            this.userNotificationStore = userNotificationStore;
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

        public async Task HandleCallbackAsync(string to, string token, SmsResult result,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("SmsChannel/HandleCallbackAsync"))
            {
                if (Guid.TryParse(token, out var id))
                {
                    var notification = await userNotificationStore.FindAsync(id, ct);

                    if (notification != null)
                    {
                        await UpdateAsync(to, result, notification);
                    }

                    if (result == SmsResult.Delivered)
                    {
                        userNotificationQueue.Complete(SmsJob.ComputeScheduleKey(id));
                    }
                }
            }
        }

        private async Task UpdateAsync(string to, SmsResult result, UserNotification notification)
        {
            if (!notification.Channels.TryGetValue(Name, out var channel))
            {
                return;
            }

            if (channel.Status.TryGetValue(to, out var status) && status.Status == ProcessStatus.Attempt)
            {
                switch (result)
                {
                    case SmsResult.Delivered:
                        await UpdateAsync(notification, to, ProcessStatus.Handled);
                        break;
                    case SmsResult.Failed:
                        await UpdateAsync(notification, to, ProcessStatus.Failed);
                        break;
                }
            }
        }

        public async Task SendAsync(UserNotification notification, NotificationSetting setting, string configuration, SendOptions options,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("SmsChannel/SendAsync"))
            {
                if (options.IsUpdate)
                {
                    return;
                }

                var job = new SmsJob(notification, setting.Template, configuration)
                {
                    IsImmediate = setting.DelayDuration == Duration.Zero
                };

                await userNotificationQueue.ScheduleAsync(
                    job.ScheduleKey,
                    job,
                    setting.DelayDuration,
                    false, ct);
            }
        }

        public Task HandleExceptionAsync(SmsJob job, Exception ex)
        {
            return UpdateAsync(job, job.PhoneNumber, ProcessStatus.Failed);
        }

        public async Task<bool> HandleAsync(SmsJob job, bool isLastAttempt,
            CancellationToken ct)
        {
            var links = job.Links();

            var parentContext = Activity.Current?.Context ?? default;

            using (Telemetry.Activities.StartActivity("SmsChannel/HandleAsync", ActivityKind.Internal, parentContext, links: links))
            {
                // If the notification is not scheduled it is very unlikey it has been confirmed already.
                if (!job.IsImmediate && await userNotificationStore.IsConfirmedOrHandledAsync(job.Id, job.PhoneNumber, Name, ct))
                {
                    await UpdateAsync(job, job.PhoneNumber, ProcessStatus.Skipped);
                }
                else
                {
                    await SendJobAsync(job, ct);
                }

                return false;
            }
        }

        private async Task SendJobAsync(SmsJob job,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("Send"))
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

                    var senders = integrationManager.Resolve<ISmsSender>(app, job.Test).Select(x => x.Target).ToList();

                    if (senders.Count == 0)
                    {
                        await SkipAsync(job, Texts.Sms_ConfigReset);
                        return;
                    }

                    await SendCoreAsync(app, job, senders, ct);
                }
                catch (DomainException ex)
                {
                    await logStore.LogAsync(job.AppId, Name, ex.Message);
                    throw;
                }
            }
        }

        private async Task SendCoreAsync(App app, SmsJob job, List<ISmsSender> senders,
            CancellationToken ct)
        {
            var lastSender = senders[^1];

            foreach (var sender in senders)
            {
                try
                {
                    var (skip, template) = await GetTemplateAsync(
                        job.AppId,
                        job.TemplateLanguage,
                        job.TemplateName,
                        ct);

                    if (skip != null)
                    {
                        await SkipAsync(job, skip);
                    }

                    var text = smsFormatter.Format(template, job.Text);

                    var result = await sender.SendAsync(app, job.PhoneNumber, text, job.Id.ToString(), ct);

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
                    await logStore.LogAsync(job.AppId, Name, ex.Message);

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

        private async Task SkipAsync(SmsJob job, string reason)
        {
            await logStore.LogAsync(job.AppId, Name, reason);

            await UpdateAsync(job, job.PhoneNumber, ProcessStatus.Skipped);
        }

        private async Task<(string? Skip, SmsTemplate?)> GetTemplateAsync(
            string appId,
            string language,
            string? name,
            CancellationToken ct)
        {
            var (status, template) = await smsTemplateStore.GetBestAsync(appId, name, language, ct);

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
