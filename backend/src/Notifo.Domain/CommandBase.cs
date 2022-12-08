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

    public string PrincipalId { get; set; }

    public virtual bool CanCreate => false;

    public virtual bool IsUpsert => true;

    public virtual ValueTask ExecutedAsync(IServiceProvider serviceProvider)
    {
        return default;
    }
}
