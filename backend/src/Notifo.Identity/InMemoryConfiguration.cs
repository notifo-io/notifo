// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Notifo.Domain.Identity;
using Notifo.Identity.InMemory;
using OpenIddict.Abstractions;
using Squidex.Hosting;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Notifo.Identity
{
    public static class InMemoryConfiguration
    {
        public static IEnumerable<Claim> Claims(this IReadOnlyDictionary<string, JsonElement> properties)
        {
            foreach (var (key, value) in properties)
            {
                var values = (string[]?)new OpenIddictParameter(value);

                if (values != null)
                {
                    foreach (var claimValue in values)
                    {
                        yield return new Claim(key, claimValue);
                    }
                }
            }
        }

        private static string BuildId(string value)
        {
            const int MongoDbLength = 24;

            var sb = new StringBuilder();

            var bytes = Encoding.Unicode.GetBytes(value);

            foreach (var c in bytes)
            {
                sb.Append(c.ToString("X2", CultureInfo.InvariantCulture));

                if (sb.Length == MongoDbLength)
                {
                    break;
                }
            }

            while (sb.Length < MongoDbLength)
            {
                sb.Append('0');
            }

            return sb.ToString();
        }

        public static OpenIddictApplicationDescriptor SetAdmin(this OpenIddictApplicationDescriptor application)
        {
            application.Properties[ClaimTypes.Role] = (JsonElement)new OpenIddictParameter(NotifoRoles.HostAdmin);

            return application;
        }

        public sealed class Scopes : InMemoryScopeStore
        {
            public Scopes()
                : base(BuildScopes())
            {
            }

            private static IEnumerable<(string, OpenIddictScopeDescriptor)> BuildScopes()
            {
                yield return (BuildId(Constants.ApiId), new OpenIddictScopeDescriptor
                {
                    Name = Constants.ApiId,
                    Resources =
                    {
                        Constants.ApiId
                    }
                });
            }
        }

        public sealed class Applications : InMemoryApplicationStore
        {
            public Applications(IUrlGenerator urlGenerator, IOptions<NotifoIdentityOptions> options)
                : base(BuildClients(urlGenerator, options.Value))
            {
            }

            private static IEnumerable<(string, OpenIddictApplicationDescriptor)> BuildClients(IUrlGenerator urlGenerator, NotifoIdentityOptions identityOptions)
            {
                yield return (BuildId(Constants.FrontendClient), new OpenIddictApplicationDescriptor
                {
                    DisplayName = "React Frontend Application",
                    ClientId = Constants.FrontendClient,
                    ClientSecret = null,
                    Type = ClientTypes.Public,
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
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.Implicit,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        Permissions.ResponseTypes.IdToken,
                        Permissions.ResponseTypes.IdTokenToken,
                        Permissions.ResponseTypes.Token,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Constants.ApiScope
                    }
                });

                if (!identityOptions.IsAdminClientConfigured())
                {
                    yield break;
                }

                var adminClientId = identityOptions.AdminClientId!;

                yield return (adminClientId, new OpenIddictApplicationDescriptor
                {
                    DisplayName = "Admin Client",
                    ClientId = adminClientId,
                    ClientSecret = identityOptions.AdminClientSecret,
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.ClientCredentials,
                        Permissions.ResponseTypes.Token,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Constants.ApiScope
                    }
                });
            }
        }
    }
}
