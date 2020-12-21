// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Identity;

namespace Microsoft.AspNetCore.Authentication
{
    public static class AuthenticationBuilderServiceExtensions
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
                });
            }

            return authBuilder;
        }
    }
}
