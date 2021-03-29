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
    public struct SendOptions
    {
        public bool IsUpdate { get; init; }

        public User User { get; init; }

        public App App { get; init; }
    }
}
