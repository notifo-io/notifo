﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Domain.UserNotifications
{
    public sealed class UserNotificationChannel
    {
        public NotificationSetting Setting { get; init; }

        public Dictionary<string, ChannelSendInfo> Status { get; init; }

        public Instant? FirstConfirmed { get; set; }

        public Instant? FirstSeen { get; set; }

        public Instant? FirstDelivered { get; set; }

        public static UserNotificationChannel Create(NotificationSetting? setting = null)
        {
            return new UserNotificationChannel
            {
                Setting = setting ?? new NotificationSetting(),
                Status = new Dictionary<string, ChannelSendInfo>()
            };
        }
    }
}
