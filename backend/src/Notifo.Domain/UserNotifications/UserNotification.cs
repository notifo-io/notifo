// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using NodaTime;

namespace Notifo.Domain.UserNotifications
{
    public sealed class UserNotification : BaseUserNotification
    {
        public bool IsDeleted { get; set; }

        public HandledInfo? IsConfirmed { get; set; }

        public HandledInfo? IsSeen { get; set; }

        public Instant Created { get; set; }

        public Instant Updated { get; set; }

        public Dictionary<string, UserNotificationChannel> Channels { get; set; }
    }
}
