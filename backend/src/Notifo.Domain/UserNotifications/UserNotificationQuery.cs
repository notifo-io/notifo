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
        public enum SearchScope
        {
            NonDeleted,
            All,
            Deleted
        }

        public Instant After { get; set; }

        public SearchScope Scope { get; set; }
    }
}
