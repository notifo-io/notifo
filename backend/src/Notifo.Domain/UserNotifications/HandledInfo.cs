// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Domain.UserNotifications
{
    public sealed class HandledInfo
    {
        public Instant Timestamp { get; set; }

        public string? Channel { get; set; }
    }
}
