// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Notifo.Domain.Apps;

namespace Notifo.Identity.Dynamic;

public sealed class DynamicSchemeProvider : AuthenticationSchemeProvider, IOptionsMonitor<DynamicOpenIdConnectOptions>
{
    private static readonly string[] UrlPrefixes = ["signin-", "signout-callback-", "signout-"];

    private readonly IAppStore appStore;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly OpenIdConnectPostConfigureOptions configure;

    public DynamicOpenIdConnectOptions CurrentValue => null!;

    public DynamicSchemeProvider(IAppStore appStore, IHttpContextAccessor httpContextAccessor, 
        OpenIdConnectPostConfigureOptions configure,
        IOptions<AuthenticationOptions> options)
        : base(options)
    {
        this.appStore = appStore;
        this.httpContextAccessor = httpContextAccessor;
        this.configure = configure;
    }

    public Task<bool> HasCustomSchemeAsync()
    {
        return appStore.AnyAuthDomainAsync(default);
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

        var app = await appStore.GetByAuthDomainAsync(parts[1], default);

        if (app?.AuthScheme != null)
        {
            return CreateScheme(app.Id, app.AuthScheme).Scheme;
        }

        return null;
    }

    public override async Task<AuthenticationScheme?> GetSchemeAsync(string name)
    {
        var result = await GetSchemeCoreAsync(name);

        if (result != null)
        {
            return result.Scheme;
        }

        return await base.GetSchemeAsync(name);
    }

    public override async Task<IEnumerable<AuthenticationScheme>> GetRequestHandlerSchemesAsync()
    {
        var result = (await base.GetRequestHandlerSchemesAsync()).ToList();

        if (httpContextAccessor.HttpContext == null)
        {
            return result;
        }

        var path = httpContextAccessor.HttpContext.Request.Path.Value;

        if (string.IsNullOrWhiteSpace(path))
        {
            return result;
        }

        var lastSegment = path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;

        foreach (var prefix in UrlPrefixes)
        {
            if (lastSegment.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var name = lastSegment[prefix.Length..];

                var scheme = await GetSchemeCoreAsync(name);
                if (scheme != null)
                {
                    result.Add(scheme.Scheme);
                }
            }
        }

        return result;
    }

    public DynamicOpenIdConnectOptions Get(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new DynamicOpenIdConnectOptions();
        }

        var scheme = GetSchemeCoreAsync(name).Result;

        return scheme?.Options ?? new DynamicOpenIdConnectOptions();
    }

    public IDisposable? OnChange(Action<DynamicOpenIdConnectOptions, string?> listener)
    {
        return null;
    }

    private async Task<SchemeResult?> GetSchemeCoreAsync(string name)
    {
        var cacheKey = ("DYNAMIC_SCHEME", name);

        if (httpContextAccessor.HttpContext?.Items.TryGetValue(cacheKey, out var cached) == true)
        {
            return cached as SchemeResult;
        }

        var app = await appStore.GetAsync(name, default);

        var result = (SchemeResult?)null;
        if (app?.AuthScheme != null)
        {
            result = CreateScheme(app.Id, app.AuthScheme);
        }

        if (httpContextAccessor.HttpContext != null)
        {
            httpContextAccessor.HttpContext.Items[cacheKey] = result;
        }

        return result;
    }

    private SchemeResult CreateScheme(string name, AppAuthScheme config)
    {
        var scheme = new AuthenticationScheme(name, config.DisplayName, typeof(DynamicOpenIdConnectHandler));

        var options = new DynamicOpenIdConnectOptions
        {
            Events = new OidcHandler(new OdicOptions
            {
                SignoutRedirectUrl = config.SignoutRedirectUrl
            }),
            Authority = config.Authority,
            CallbackPath = new PathString($"/signin-{name}"),
            ClientId = config.ClientId,
            ClientSecret = config.ClientSecret,
            RemoteSignOutPath = new PathString($"/signout-{name}"),
            RequireHttpsMetadata = false,
            ResponseType = "code",
            SignedOutRedirectUri = new PathString($"/signout-callback-{name}")
        };

        configure.PostConfigure(name, options);

        return new SchemeResult(scheme, options);
    }

#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
    private sealed record SchemeResult(AuthenticationScheme Scheme, DynamicOpenIdConnectOptions Options);
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
#pragma warning restore RECS0082 // Parameter has the same name as a member and hides it
}
