// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Infrastructure;

namespace Notifo.Domain.UserNotifications
{
    public sealed class UserNotificationQuery : QueryBase
    {
        public Instant After { get; set; }

        public UserNotificationQueryScope Scope { get; set; }

        public string? Query { get; set; }

        public UserNotificationQuery()
        {
            TotalNeeded = false;
        }
    }
}
