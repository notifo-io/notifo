// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Notifo.Domain.Identity;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Identity;

internal sealed class UserWithClaims : IUser
{
    private readonly IdentityUser snapshot;

    public IdentityUser Identity { get; }

    public string Id
    {
        get => snapshot.Id;
    }

    public string Email
    {
        get => snapshot.Email!;
    }

    public bool IsLocked
    {
        get => snapshot.LockoutEnd > DateTimeOffset.UtcNow;
    }

    public IReadOnlyList<Claim> Claims { get; }

    public IReadOnlySet<string> Roles { get; }

    object IUser.Identity => Identity;

    public UserWithClaims(IdentityUser user, IReadOnlyList<Claim> claims, IReadOnlySet<string> roles)
    {
        Identity = user;

        // Clone the user so that we capture the previous values, even when the user is updated.
        snapshot = SimpleMapper.Map(user, new IdentityUser());

        // Claims are immutable so we do not need a copy of them.
        Claims = claims;

        // Roles are immutable so we do not need a copy of them.
        Roles = roles;
    }
}
