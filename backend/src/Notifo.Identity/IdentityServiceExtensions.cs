// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Notifo.Domain.Apps;
using Notifo.Domain.Identity;
using Notifo.Identity;
using Notifo.Identity.ApiKey;
using Notifo.Identity.Dynamic;
using Notifo.Identity.InMemory;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Microsoft.Extensions.DependencyInjection;

public static class IdentityServiceExtensions
{
    public static void AddMyIdentity(this IServiceCollection services, IConfiguration config)
    {
        IdentityModelEventSource.ShowPII = true;

        var identityOptions = config.GetSection("identity").Get<NotifoIdentityOptions>() ?? new NotifoIdentityOptions();

        services.Configure<NotifoIdentityOptions>(config, "identity");

        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddClaimsPrincipalFactory<ClaimFactory>()
            .AddDefaultTokenProviders();

        services.AddSingletonAs<UserCreator>()
            .AsSelf();

        services.AddSingletonAs<TokenStoreCleaner>()
            .AsSelf();

        services.AddSingletonAs<OpenIdConnectPostConfigureOptions>()
            .AsSelf();

        services.AddSingletonAs<DynamicSchemeProvider>()
            .AsSelf().As<IAuthenticationSchemeProvider>().As<IOptionsMonitor<DynamicOpenIdConnectOptions>>();

        services.AddSingletonAs<DefaultUserResolver>()
            .As<IUserResolver>();

        services.AddScopedAs<DefaultUserService>()
            .As<IUserService>();

        services.Configure<KeyManagementOptions>((c, options) =>
        {
            options.XmlRepository = c.GetRequiredService<IXmlRepository>();
        });

        services.AddMyOpenIdDict();
        services.AddAuthorization();
        services.AddAuthentication()
           .AddPolicyScheme(Constants.IdentityServerOrApiKeyScheme, null, options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    if (ApiKeyHandler.IsApiKey(context.Request, out _))
                    {
                        return ApiKeyDefaults.AuthenticationScheme;
                    }

                    return OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                };
            })
            .AddGoogle(identityOptions)
            .AddGithub(identityOptions)
            .AddOidc(identityOptions)
            .AddApiKey();
    }

    private static void AddMyOpenIdDict(this IServiceCollection services)
    {
        services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
            options.ClaimsIdentity.UserNameClaimType = Claims.Name;
            options.ClaimsIdentity.RoleClaimType = Claims.Role;
        });

        services.AddOpenIddict()
            .AddCore(builder =>
            {
                builder.SetDefaultScopeEntity<ImmutableScope>();

                builder.Services.AddSingletonAs<InMemoryConfiguration.Scopes>()
                    .As<IOpenIddictScopeStore<ImmutableScope>>();

                builder.SetDefaultApplicationEntity<ImmutableApplication>();

                builder.Services.AddSingletonAs<InMemoryConfiguration.Applications>()
                    .As<IOpenIddictApplicationStore<ImmutableApplication>>();

                builder.ReplaceApplicationManager(typeof(ApplicationManager<>));
            })
            .AddServer(builder =>
            {
                builder
                    .SetAuthorizationEndpointUris("/connect/authorize")
                    .SetIntrospectionEndpointUris("/connect/introspect")
                    .SetEndSessionEndpointUris("/connect/logout")
                    .SetTokenEndpointUris("/connect/token")
                    .SetUserInfoEndpointUris("/connect/userinfo");

                builder.RegisterScopes(
                    Scopes.Email,
                    Scopes.Profile,
                    Scopes.Roles,
                    Constants.ApiScope);

                builder.AllowImplicitFlow();
                builder.AllowAuthorizationCodeFlow();
                builder.AllowClientCredentialsFlow();

                builder.SetAccessTokenLifetime(TimeSpan.FromDays(30));

                builder.UseAspNetCore()
                    .DisableTransportSecurityRequirement()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableStatusCodePagesIntegration()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough();
            })
            .AddValidation(builder =>
            {
                builder.UseLocalServer();
                builder.UseAspNetCore();
            });
    }
}
