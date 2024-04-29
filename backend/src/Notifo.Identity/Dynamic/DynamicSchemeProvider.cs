// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Notifo.Domain.Apps;

namespace Notifo.Identity.Dynamic;

public sealed class DynamicSchemeProvider : AuthenticationSchemeProvider
{
    private readonly IAppStore appStore;
    private readonly IOptionsMonitorCache<OpenIdConnectOptions> optionsCache;

    public DynamicSchemeProvider(IAppStore appStore,
        IOptions<AuthenticationOptions> options,
        IOptionsMonitorCache<OpenIdConnectOptions> optionsCache)
        : base(options)
    {
        this.appStore = appStore;
        this.optionsCache = optionsCache;
    }

    public Task<bool> HasCustomSchemeAsync()
    {
        return appStore.AnyAuthDomainAsync();
    }

    public async Task<AuthenticationScheme?> GetSchemaByEmailAddressAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var parts = email.Split('@');

        if (parts.Length != 2)
        {
            return null;
        }

        var app = await appStore.GetByAuthDomainAsync(parts[1]);

        if (app?.AuthScheme != null)
        {
            return CreateScheme(app.Id, app.AuthScheme);
        }

        return null;
    }

    public override async Task<AuthenticationScheme?> GetSchemeAsync(string name)
    {
        var app = await appStore.GetAsync(name);

        if (app?.AuthScheme != null)
        {
            return CreateScheme(app.Id, app.AuthScheme);
        }

        return await base.GetSchemeAsync(name);
    }

    private AuthenticationScheme CreateScheme(string name, AppAuthScheme config)
    {
        optionsCache.TryAdd(name, new OpenIdConnectOptions
        {
            Events = new OidcHandler(new OdicOptions
            {
                SignoutRedirectUrl = config.SignoutRedirectUrl
            }),
            Authority = config.Authority,
            ClientId = config.ClientId,
            ClientSecret = config.ClientSecret,
        });

        return new AuthenticationScheme(name, config.DisplayName, typeof(OpenIdConnectHandler));
    }
}
