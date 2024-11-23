// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication.OpenIdConnect;

#pragma warning disable MA0048 // File name must match type name

namespace Notifo.Identity;

public class OidcOptions
{
    public string? SignoutRedirectUrl { get; set; }
}

public sealed class OidcHandler(OidcOptions options) : OpenIdConnectEvents
{
    public override Task RedirectToIdentityProviderForSignOut(RedirectContext context)
    {
        if (!string.IsNullOrEmpty(options.SignoutRedirectUrl))
        {
            var logoutUri = options.SignoutRedirectUrl;

            context.Response.Redirect(logoutUri);
            context.HandleResponse();

            return Task.CompletedTask;
        }

        return base.RedirectToIdentityProviderForSignOut(context);
    }
}
