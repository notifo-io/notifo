// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using NodaTime;

namespace Notifo.Domain;

public abstract class CommandBase
{
    public Instant Timestamp { get; set; }

    public ClaimsPrincipal Principal { get; set; }

    public string? PrincipalId { get; set; }

    public virtual bool CanCreate => false;

    public virtual bool IsUpsert => true;

    public virtual ValueTask<T?> ExecuteAsync(T target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        return default;
    }

    public virtual ValueTask ExecuteAsync(IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        return default;
    }

    public static ClaimsPrincipal BackendUser(string userId)
    {
        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        claimsIdentity.AddClaim(new Claim("sub", userId));

        return claimsPrincipal;
    }
}
