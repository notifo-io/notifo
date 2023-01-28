// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using OpenNotifications;
using Squidex.Hosting;

namespace Notifo.Domain.Integrations.OpenNotifications;

public sealed class OpenNotificationsRegistry : IIntegrationRegistry, IInitializable
{
    private readonly ConcurrentDictionary<string, IIntegration> integrations = new ConcurrentDictionary<string, IIntegration>();
    private readonly IEnumerable<IOpenNotificationsClient> clients;

    public IEnumerable<IIntegration> Integrations => integrations.Values;

    public OpenNotificationsRegistry(IEnumerable<IOpenNotificationsClient> clients)
    {
        this.clients = clients;
    }

    public bool TryGetIntegration(string type, [MaybeNullWhen(false)] out IIntegration integration)
    {
        return integrations.TryGetValue(type, out integration);
    }

    public async Task InitializeAsync(
        CancellationToken ct)
    {
        foreach (var client in clients)
        {
            var request = new GetProvidersRequestDto
            {
                Context = new RequestContextDto()
            };

            var providers = await client.Providers.GetProvidersAsync(request, ct);

            foreach (var (name, providerInfo) in providers.Providers)
            {
                var fullName = $"{client.Name}_{name}";

                integrations.TryAdd(fullName, new OpenNotificationsIntegration(fullName, name, providerInfo, client));
            }
        }
    }
}
