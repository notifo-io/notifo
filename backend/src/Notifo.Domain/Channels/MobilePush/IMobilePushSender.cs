﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.MobilePush
{
    public interface IMobilePushSender
    {
        Task SendAsync(UserNotification userNotification, MobilePushOptions options,
            CancellationToken ct);
    }
}
