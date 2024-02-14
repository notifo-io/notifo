// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Notifo.Identity;

public sealed class OidcHandler : OpenIdConnectEvents
{
    private readonly NotifoIdentityOptions options;

    public OidcHandler(NotifoIdentityOptions options)
    {
        this.options = options;
    }

    public override Task RedirectToIdentityProviderForSignOut(RedirectContext context)
    {
        if (!string.IsNullOrEmpty(options.OidcOnSignoutRedirectUrl))
        {
            var logoutUri = options.OidcOnSignoutRedirectUrl;

            context.Response.Redirect(logoutUri);
            context.HandleResponse();

            return Task.CompletedTask;
        }

        return base.RedirectToIdentityProviderForSignOut(context);
    }
}
