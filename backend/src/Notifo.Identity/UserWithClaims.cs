// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Notifo.Domain.Identity;

#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Identity;

internal sealed record UserWithClaims(IdentityUser Identity, IReadOnlyList<Claim> Claims, IReadOnlySet<string> Roles) : IUser
{
    public string Id
    {
        get => Identity.Id;
    }

    public string Email
    {
        get => Identity.Email;
    }

    public bool IsLocked
    {
        get => Identity.LockoutEnd > DateTime.UtcNow;
    }

    object IUser.Identity => Identity;
}
