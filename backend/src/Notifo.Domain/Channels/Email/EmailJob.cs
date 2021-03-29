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
        public UserNotification Notification { get; set; }

        public string EmailAddress { get; set; }

        public string ScheduleKey
        {
            get => $"{Notification.AppId}_{Notification.UserId}_{Notification.UserLanguage}_{EmailAddress}";
        }

        public EmailJob()
        {
        }

        public EmailJob(UserNotification notification, string emailAddress)
        {
            Notification = notification;

            EmailAddress = emailAddress;
        }
    }
}
