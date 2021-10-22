// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Email
{
    public sealed class EmailJob
    {
        public BaseUserNotification Notification { get; set; }

        public string EmailAddress { get; set; }

        public string? EmailTemplate { get; set; }

        public string? FromEmail { get; set; }

        public string? FromName { get; set; }

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

        public EmailJob(BaseUserNotification notification, NotificationSetting setting, string emailAddress)
        {
            EmailAddress = emailAddress;
            EmailTemplate = setting.Template;
            FromEmail = setting.Properties?.GetOrDefault(nameof(FromEmail));
            FromName = setting.Properties?.GetOrDefault(nameof(FromName));
            Notification = notification;
        }
    }
}
