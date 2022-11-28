// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Domain.Users;

namespace Notifo.Domain.Channels;

public readonly struct SendContext
{
    required public App App { get; init; }

    required public User User { get; init; }

    required public string AppId { get; init; }

    required public string UserId { get; init; }

    required public bool IsUpdate { get; init; }
}
