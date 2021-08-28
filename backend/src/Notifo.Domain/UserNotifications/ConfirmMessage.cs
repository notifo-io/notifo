// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Notifo.Domain.UserNotifications
{
    public sealed class ConfirmMessage
    {
        public Guid Id { get; set; }

        public TrackingDetails Details { get; set; }
    }
}
