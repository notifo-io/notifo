// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.Email
{
    public sealed class EmailJob
    {
        public BaseUserNotification Notification { get; set; }

        public string EmailAddress { get; set; }

        public string? EmailTemplate { get; set; }

        public string ScheduleKey
        {
            get => $"{Notification.AppId}_{Notification.UserId}_{Notification.UserLanguage}_{Notification.Test}_{EmailAddress}_{EmailTemplate}";
        }

        public EmailJob()
        {
        }

        public EmailJob(BaseUserNotification notification, string? emailTemplate, string emailAddress)
        {
            Notification = notification;
            EmailAddress = emailAddress;
            EmailTemplate = emailTemplate;
        }
    }
}
