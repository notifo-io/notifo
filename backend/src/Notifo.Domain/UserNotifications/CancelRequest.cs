// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.UserNotifications;

public sealed class CancelRequest
{
    public string AppId { get; set; }

    public string UserId { get; set; }

    public string EventId { get; set; }

    public string? GroupKey { get; set; }

    public bool Test { get; set; }
}
