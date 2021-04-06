// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using Squidex.Hosting;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Notifo.Identity
{
    public sealed class OpenIdDictWorker : IHostedService
    {
        private readonly IServiceProvider serviceProvider;

        public OpenIdDictWorker(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                await CreateApplicationsAsync();
                await CreateScopesAsync();

                async Task CreateApplicationsAsync()
                {
                    var urlGenerator = scope.ServiceProvider.GetRequiredService<IUrlGenerator>();

                    var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

                    var client = await manager.FindByClientIdAsync("notifo", cancellationToken);

                    if (client != null)
                    {
                        var redirectUris = await manager.GetRedirectUrisAsync(client, cancellationToken);

                        if (urlGenerator.IsAllowedHost(redirectUris[0]))
                        {
                            return;
                        }
                    }

                    var descriptor = new OpenIddictApplicationDescriptor
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

                    if (client != null)
                    {
                        await manager.UpdateAsync(Constants.FrontendClient, descriptor, cancellationToken);
                    }
                    else
                    {
                        await manager.CreateAsync(descriptor, cancellationToken);
                    }
                }

                async Task CreateScopesAsync()
                {
                    var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

                    if (await manager.FindByNameAsync(Constants.ApiId, cancellationToken) == null)
                    {
                        var descriptor = new OpenIddictScopeDescriptor
                        {
                            Name = Constants.ApiId,
                            Resources =
                            {
                                Constants.ApiId
                            }
                        };

                        await manager.CreateAsync(descriptor, cancellationToken);
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
