// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Identity;

namespace Microsoft.AspNetCore.Authentication;

public static class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddGoogle(this AuthenticationBuilder authBuilder, NotifoIdentityOptions identityOptions)
    {
        if (identityOptions.IsGoogleAuthConfigured())
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = identityOptions.GoogleClient;
                options.ClientSecret = identityOptions.GoogleSecret;
            });
        }

        return authBuilder;
    }

    public static AuthenticationBuilder AddGithub(this AuthenticationBuilder authBuilder, NotifoIdentityOptions identityOptions)
    {
        if (identityOptions.IsGithubAuthConfigured())
        {
            authBuilder.AddGitHub(options =>
            {
                options.ClientId = identityOptions.GithubClient;
                options.ClientSecret = identityOptions.GithubSecret;
                options.Scope.Add("user:email");
            });
        }

        return authBuilder;
    }

    public static AuthenticationBuilder AddOidc(this AuthenticationBuilder authBuilder, NotifoIdentityOptions identityOptions)
    {
        if (identityOptions.IsOidcConfigured())
        {
            var displayName = !string.IsNullOrWhiteSpace(identityOptions.OidcName) ? identityOptions.OidcName : OpenIdConnectDefaults.DisplayName;

            authBuilder.AddOpenIdConnect("ExternalOidc", displayName, options =>
            {
                options.Authority = identityOptions.OidcAuthority;
                options.ClientId = identityOptions.OidcClient;
                options.ClientSecret = identityOptions.OidcSecret;
                options.RequireHttpsMetadata = identityOptions.RequiresHttps;
                options.Events = new OidcHandler(identityOptions);

                if (!string.IsNullOrEmpty(identityOptions.OidcMetadataAddress))
                {
                    options.MetadataAddress = identityOptions.OidcMetadataAddress;
                }

                if (!string.IsNullOrEmpty(identityOptions.OidcResponseType))
                {
                    options.ResponseType = identityOptions.OidcResponseType;
                }

                options.GetClaimsFromUserInfoEndpoint = identityOptions.OidcGetClaimsFromUserInfoEndpoint;

                if (identityOptions.OidcScopes != null)
                {
                    foreach (var scope in identityOptions.OidcScopes)
                    {
                        options.Scope.Add(scope);
                    }
                }
            });
        }

        return authBuilder;
    }
}
