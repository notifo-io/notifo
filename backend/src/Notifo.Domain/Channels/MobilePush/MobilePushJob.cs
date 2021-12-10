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
        public BaseUserNotification Notification { get; init; }

        public string DeviceToken { get; init; }

        public MobileDeviceType DeviceType { get; init; }

        public bool IsImmediate { get; init; }

        public bool IsUpdate { get; init; }

        public bool IsConfirmed { get; set; }

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
