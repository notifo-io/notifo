// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.Messaging
{
    public sealed class MessagingJob : ChannelJob
    {
        public BaseUserNotification Notification { get; init; }

        public string? NotificationTemplate { get; init; }

        public Dictionary<string, string> Targets { get; init; } = new Dictionary<string, string>();

        public string ScheduleKey
        {
            get => ComputeScheduleKey(Notification.Id);
        }

        public MessagingJob()
        {
        }

        public MessagingJob(BaseUserNotification notification, ChannelSetting setting, Guid configurationId)
            : base(notification, setting, configurationId, false, Providers.Email)
        {
            Notification = notification;
            NotificationTemplate = setting.Template;
        }

        public static string ComputeScheduleKey(Guid notificationId)
        {
            return $"{notificationId}";
        }
    }
}
