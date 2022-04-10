// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Domain.UserNotifications
{
    public sealed record UserNotificationChannel
    {
        public ChannelSetting Setting { get; init; } = new ChannelSetting();

        public Dictionary<string, ChannelSendInfo> Status { get; init; } = new Dictionary<string, ChannelSendInfo>();

        public Instant? FirstConfirmed { get; init; }

        public Instant? FirstSeen { get; init; }

        public Instant? FirstDelivered { get; init; }
    }
}
