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

internal sealed class UserWithClaims(
    IdentityUser user,
    IReadOnlyList<Claim> claims,
    IReadOnlySet<string> roles,
    bool hasLoginOrPassword)
    :  IUser
{
    private readonly IdentityUser snapshot = SimpleMapper.Map(user, new IdentityUser());

    public IdentityUser Identity { get; } = user;

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

    public bool HasLoginOrPassword { get; } = hasLoginOrPassword;

    public IReadOnlyList<Claim> Claims { get; } = claims;

    public IReadOnlySet<string> Roles { get; } = roles;

    object IUser.Identity => Identity;
}
