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

#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Identity.Dynamic;

public sealed class DynamicSchemeProvider(
    IAppStore appStore,
    IHttpContextAccessor httpContextAccessor,
    IConfigurationStore<AppAuthScheme> temporarySchemes,
    OpenIdConnectPostConfigureOptions configure,
    IOptions<AuthenticationOptions> options)
    : AuthenticationSchemeProvider(options), IOptionsMonitor<DynamicOpenIdConnectOptions>
{
    private static readonly string[] UrlPrefixes = ["signin-", "signout-callback-", "signout-"];

    public DynamicOpenIdConnectOptions CurrentValue => null!;

    private sealed record SchemeResult(AuthenticationScheme Scheme, DynamicOpenIdConnectOptions Options);

    public async Task<string> AddTemporarySchemeAsync(AppAuthScheme scheme,
        CancellationToken ct = default)
    {
        var id = Guid.NewGuid().ToString();

        await temporarySchemes.SetAsync(id, scheme, TimeSpan.FromMinutes(10), ct);
        return id;
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
        var result = await GetSchemeCoreAsync(name, default);

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

                var scheme = await GetSchemeCoreAsync(name, httpContextAccessor.HttpContext.RequestAborted);

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

        var scheme = GetSchemeCoreAsync(name, default).Result;

        return scheme?.Options ?? new DynamicOpenIdConnectOptions();
    }

    private async Task<SchemeResult?> GetSchemeCoreAsync(string name,
        CancellationToken ct)
    {
        if (!Guid.TryParse(name, out _))
        {
            return null;
        }

        var cacheKey = ("DYNAMIC_SCHEME", name);

        if (httpContextAccessor.HttpContext?.Items.TryGetValue(cacheKey, out var cached) == true)
        {
            return cached as SchemeResult;
        }

        var scheme =
            await GetSchemeByAppAsync(name, ct) ??
            await GetSchemeByTempNameAsync(name, ct);

        var result =
            scheme != null ?
            CreateScheme(name, scheme) :
            null;

        if (httpContextAccessor.HttpContext != null)
        {
            httpContextAccessor.HttpContext.Items[cacheKey] = result;
        }

        return result;
    }

    private async Task<AppAuthScheme?> GetSchemeByAppAsync(string name,
        CancellationToken ct)
    {
        var app = await appStore.GetByAuthDomainAsync(name, ct);

        return app?.AuthScheme;
    }

    private async Task<AppAuthScheme?> GetSchemeByTempNameAsync(string name,
        CancellationToken ct)
    {
        var scheme = await temporarySchemes.GetAsync(name, ct);

        return scheme;
    }

    private SchemeResult CreateScheme(string name, AppAuthScheme config)
    {
        var scheme = new AuthenticationScheme(name, config.DisplayName, typeof(DynamicOpenIdConnectHandler));

        var openIdOptions = new DynamicOpenIdConnectOptions
        {
            Events = new OidcHandler(new OidcOptions
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

        configure.PostConfigure(name, openIdOptions);

        return new SchemeResult(scheme, openIdOptions);
    }

    public IDisposable? OnChange(Action<DynamicOpenIdConnectOptions, string?> listener)
    {
        return null;
    }
}
