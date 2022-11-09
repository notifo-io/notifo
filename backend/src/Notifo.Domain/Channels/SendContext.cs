// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Domain.Users;

namespace Notifo.Domain.Channels
{
    public readonly struct SendContext
    {
        public App App { get; init; }

        public User User { get; init; }

        public string AppId { get; init; }

        public string UserId { get; init; }

        public bool IsUpdate { get; init; }
    }
}
