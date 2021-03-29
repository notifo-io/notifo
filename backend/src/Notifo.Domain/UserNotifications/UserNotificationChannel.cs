// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Notifo.Domain.UserNotifications
{
    public sealed class UserNotificationChannel
    {
        public NotificationSetting Setting { get; set; }

        public Dictionary<string, ChannelSendInfo> Status { get; set; }

        public static UserNotificationChannel Create(NotificationSetting setting)
        {
            return new UserNotificationChannel
            {
                Setting = setting,
                Status = new Dictionary<string, ChannelSendInfo>()
            };
        }
    }
}
