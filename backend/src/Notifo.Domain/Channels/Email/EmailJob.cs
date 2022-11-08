// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Email
{
    public sealed class EmailJob : IChannelJob
    {
        public BaseUserNotification Notification { get; init; }

        public string EmailAddress { get; init; }

        public string? EmailTemplate { get; init; }

        public string? FromEmail { get; init; }

        public string? FromName { get; init; }

        public Guid ConfigurationId { get; init; }

        public ChannelCondition Condition { get; init; }

        public Duration Delay { get; init; }

        Guid IChannelJob.NotificationId
        {
            get => Notification.Id;
        }

        public string ScheduleKey
        {
            get => string.Join("_",
                Notification.AppId,
                Notification.UserId,
                Notification.UserLanguage,
                Notification.Test,
                EmailAddress,
                EmailTemplate,
                FromEmail,
                FromName);
        }

        public EmailJob()
        {
        }

        public EmailJob(BaseUserNotification notification, ChannelSetting setting, Guid configurationId, string emailAddress)
        {
            Delay = Duration.FromSeconds(setting.DelayInSeconds ?? 0);
            EmailAddress = emailAddress;
            EmailTemplate = setting.Template;
            Condition = setting.Condition;
            ConfigurationId = configurationId;
            FromEmail = setting.Properties?.GetOrDefault(nameof(FromEmail));
            FromName = setting.Properties?.GetOrDefault(nameof(FromName));
            Notification = notification;
        }
    }
}
