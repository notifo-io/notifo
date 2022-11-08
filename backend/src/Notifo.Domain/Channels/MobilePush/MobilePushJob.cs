// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.MobilePush
{
    public sealed class MobilePushJob : IChannelJob
    {
        public BaseUserNotification Notification { get; init; }

        public string DeviceToken { get; init; }

        public MobileDeviceType DeviceType { get; init; }

        public bool IsConfirmed { get; init; }

        public bool IsUpdate { get; init; }

        public Guid ConfigurationId { get; init; }

        public ChannelCondition Condition { get; init; }

        public Duration Delay { get; init; }

        Guid IChannelJob.NotificationId
        {
            get => Notification.Id;
        }

        public string ScheduleKey
        {
            get => $"{Notification.Id}_{DeviceToken}";
        }

        public MobilePushJob()
        {
        }

        public MobilePushJob(UserNotification notification, ChannelSetting? setting, Guid configurationId, string token, MobileDeviceType deviceType, bool isUpdate)
        {
            Condition = setting?.Condition ?? ChannelCondition.Always;
            ConfigurationId = configurationId;
            Delay = Duration.FromSeconds(setting?.DelayInSeconds ?? 0);
            DeviceToken = token;
            DeviceType = deviceType;
            IsConfirmed = notification.FirstConfirmed != null;
            IsUpdate = isUpdate;
            Notification = notification;
        }
    }
}
