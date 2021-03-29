// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.MobilePush
{
    public sealed class MobilePushJob
    {
        public UserNotification Notification { get; set; }

        public string Token { get; set; }

        public bool IsImmediate { get; set; }

        public string ScheduleKey
        {
            get => $"{Notification.Id}_{Token}";
        }

        public MobilePushJob()
        {
        }

        public MobilePushJob(UserNotification notification, string token)
        {
            Notification = notification;

            Token = token;
        }
    }
}
