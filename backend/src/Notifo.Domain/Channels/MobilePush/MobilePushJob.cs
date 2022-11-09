// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.MobilePush
{
    public sealed class MobilePushJob : ChannelJob
    {
        public BaseUserNotification Notification { get; init; }

        public string DeviceToken { get; init; }

        public MobileDeviceType DeviceType { get; init; }

        public bool IsConfirmed { get; init; }

        public string ScheduleKey
        {
            get => $"{Notification.Id}_{DeviceToken}";
        }

        public MobilePushJob()
        {
        }

        public MobilePushJob(UserNotification notification, ChannelSetting? setting, Guid configurationId, string token, MobileDeviceType type, bool isUpdate)
            : base(notification, setting, configurationId, isUpdate, Providers.MobilePush)
        {
            DeviceToken = token;
            DeviceType = type;
            IsConfirmed = notification.FirstConfirmed != null;
            Notification = notification;
        }
    }
}
