// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Notifo.Infrastructure.Timers;
using OpenNotifications;
using Squidex.Hosting;

namespace Notifo.Domain.Integrations.OpenNotifications;

public sealed class OpenNotificationsRegistry(IEnumerable<IOpenNotificationsClient> clients, ILogger<OpenNotificationsRegistry> log) : IIntegrationRegistry, IBackgroundProcess
{
    private static readonly HashSet<string> ProvidersToIgnore =
    [
        "aws-email",
        "mailjet",
        "mailjet-smtp",
        "messagebird-sms",
        "smtp",
        "twilio-sms",
    ];
    private Dictionary<string, IIntegration> integrations = [];
    private CompletionTimer timer;

    public IEnumerable<IIntegration> Integrations => integrations.Values;

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

            integrations = BuildIntegrations(client, providers);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Failed to query providers from {name}", client.Name);
        }
    }

    private static Dictionary<string, IIntegration> BuildIntegrations(IOpenNotificationsClient client, GetProvidersResponseDto providers)
    {
        var newIntegrations = new Dictionary<string, IIntegration>();

        foreach (var (name, providerInfo) in providers.Providers)
        {
            if (ProvidersToIgnore.Contains(name))
            {
                continue;
            }

            var fullName = $"{client.Name}_{name}";

            if (providerInfo.Type == ProviderInfoDtoType.Sms)
            {
                newIntegrations.TryAdd(fullName, new OpenNotificationsSmsIntegration(fullName, name, providerInfo, client));
            }
            else if (providerInfo.Type == ProviderInfoDtoType.Email)
            {
                newIntegrations.TryAdd(fullName, new OpenNotificationsEmailIntegration(fullName, name, providerInfo, client));
            }
        }

        return newIntegrations;
    }

    public bool TryGetIntegration(string type, [MaybeNullWhen(false)] out IIntegration integration)
    {
        return integrations.TryGetValue(type, out integration);
    }
}
