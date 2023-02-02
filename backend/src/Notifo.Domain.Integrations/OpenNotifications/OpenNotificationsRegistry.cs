// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Notifo.Infrastructure.Timers;
using OpenNotifications;
using Squidex.Hosting;

namespace Notifo.Domain.Integrations.OpenNotifications;

public sealed class OpenNotificationsRegistry : IIntegrationRegistry, IBackgroundProcess
{
    private readonly ConcurrentDictionary<string, IIntegration> integrations = new ConcurrentDictionary<string, IIntegration>();
    private readonly IEnumerable<IOpenNotificationsClient> clients;
    private readonly ILogger<OpenNotificationsRegistry> log;
    private CompletionTimer timer;

    public IEnumerable<IIntegration> Integrations => integrations.Values;

    public OpenNotificationsRegistry(IEnumerable<IOpenNotificationsClient> clients, ILogger<OpenNotificationsRegistry> log)
    {
        this.clients = clients;
        this.log = log;
    }

    public Task StartAsync(
        CancellationToken ct)
    {
        timer = new CompletionTimer(5 * 60 * 1000, QueryAsync);

        return Task.CompletedTask;
    }

    public Task StopAsync(
        CancellationToken ct)
    {
        return timer?.StopAsync() ?? Task.CompletedTask;
    }

    public async Task QueryAsync(
        CancellationToken ct)
    {
        foreach (var client in clients)
        {
            await QueryClientAsync(client, ct);
        }
    }

    private async Task QueryClientAsync(IOpenNotificationsClient client, CancellationToken ct)
    {
        try
        {
            var request = new GetProvidersRequestDto
            {
                Context = new RequestContextDto()
            };

            var providers = await client.Providers.GetProvidersAsync(request, ct);

            foreach (var (name, providerInfo) in providers.Providers)
            {
                var fullName = $"{client.Name}_{name}";

                if (providerInfo.Type == ProviderInfoDtoType.Sms)
                {
                    integrations.TryAdd(fullName, new OpenNotificationsSmsIntegration(fullName, name, providerInfo, client));
                }
                else if (providerInfo.Type == ProviderInfoDtoType.Email)
                {
                    integrations.TryAdd(fullName, new OpenNotificationsEmailIntegration(fullName, name, providerInfo, client));
                }
            }
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Failed to query providers from {name}", client.Name);
        }
    }

    public bool TryGetIntegration(string type, [MaybeNullWhen(false)] out IIntegration integration)
    {
        return integrations.TryGetValue(type, out integration);
    }
}
