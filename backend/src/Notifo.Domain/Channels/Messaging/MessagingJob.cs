// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.Messaging
{
    public sealed class MessagingJob : IChannelJob
    {
        public const string DefaultToken = "Default";

        public BaseUserNotification Notification { get; init; }

        public string? NotificationTemplate { get; }

        public Dictionary<string, string> Targets { get; init; } = new Dictionary<string, string>();

        public ChannelCondition Condition { get; init; }

        public Duration Delay { get; init; }

        Guid IChannelJob.NotificationId
        {
            get => Notification.Id;
        }

        public string Configuration
        {
            get => DefaultToken;
        }

        public string ScheduleKey
        {
            get => ComputeScheduleKey(Notification.Id);
        }

        public MessagingJob()
        {
        }

        public MessagingJob(BaseUserNotification notification, ChannelSetting setting)
        {
            Delay = Duration.FromSeconds(setting.DelayInSeconds ?? 0);
            Notification = notification;
            NotificationTemplate = setting.Template;
            Condition = setting.Condition;
        }

        public static string ComputeScheduleKey(Guid notificationId)
        {
            return $"{notificationId}";
        }
    }
}
