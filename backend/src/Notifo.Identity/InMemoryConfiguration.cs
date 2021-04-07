// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Notifo.Identity.InMemory;
using OpenIddict.Abstractions;
using Squidex.Hosting;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Notifo.Identity
{
    public static class InMemoryConfiguration
    {
        public static readonly IOpenIddictScopeStore<OpenIddictScopeDescriptor> Scopes = new InMemoryScopeStore(
            new OpenIddictScopeDescriptor
            {
                Name = Constants.ApiId,
                Resources =
                {
                    Constants.ApiId
                }
            });

        public sealed class Clients : InMemoryApplicationStore
        {
            public Clients(IUrlGenerator urlGenerator)
                : base(BuildClients(urlGenerator))
            {
            }

            private static List<OpenIddictApplicationDescriptor> BuildClients(IUrlGenerator urlGenerator)
            {
                var client = new OpenIddictApplicationDescriptor
                {
                    DisplayName = "React Frontend Application",
                    ClientId = Constants.FrontendClient,
                    ClientSecret = null,
                    PostLogoutRedirectUris =
                    {
                        new Uri(urlGenerator.BuildUrl("authentication/logout-callback", false))
                    },
                    RedirectUris =
                    {
                        new Uri(urlGenerator.BuildUrl("authentication/login-callback", false)),
                        new Uri(urlGenerator.BuildUrl("authentication/login-silent-callback.html", false))
                    },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        Permissions.GrantTypes.Implicit,
                        Permissions.ResponseTypes.IdToken,
                        Permissions.ResponseTypes.IdTokenToken,
                        Permissions.ResponseTypes.Token,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Constants.ApiScope
                    }
                };

                return new List<OpenIddictApplicationDescriptor> { client };
            }
        }
    }
}
