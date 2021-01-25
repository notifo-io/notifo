// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;

namespace Notifo.Domain.Identity
{
    public interface IUser
    {
        bool IsLocked { get; }

        string Id { get; }

        string Email { get; }

        object Identity { get; }

        IReadOnlyList<Claim> Claims { get; }
    }
}
