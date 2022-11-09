// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.UserNotifications;

public sealed class ConfirmMessage
{
    public TrackingToken Token { get; init; }
}
