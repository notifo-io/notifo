// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication.OpenIdConnect;

#pragma warning disable MA0048 // File name must match type name

namespace Notifo.Identity;

public class OdicOptions
{
    public string? SignoutRedirectUrl { get; set; }
}

public sealed class OidcHandler : OpenIdConnectEvents
{
    private readonly OdicOptions options;

    public OidcHandler(OdicOptions options)
    {
        this.options = options;
    }

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
