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

        public string DeviceToken { get; set; }

        public MobileDeviceType DeviceType { get; set; }

        public bool IsImmediate { get; set; }

        public string ScheduleKey
        {
            get => $"{Notification.Id}_{DeviceToken}";
        }

        public MobilePushJob()
        {
        }

        public MobilePushJob(UserNotification notification, string token, MobileDeviceType deviceType)
        {
            Notification = notification;

            DeviceToken = token;
            DeviceType = deviceType;
        }
    }
}
