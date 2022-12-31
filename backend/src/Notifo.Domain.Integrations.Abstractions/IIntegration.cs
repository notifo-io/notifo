// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public interface IIntegration
{
    IntegrationDefinition Definition { get; }

    object? Create(Type serviceType, string id, IntegrationConfiguration configured, IServiceProvider serviceProvider);

    bool CanCreate(Type serviceType, string id, IntegrationConfiguration configured);

    Task<IntegrationStatus> OnConfiguredAsync(AppContext app, string id, IntegrationConfiguration configured, IntegrationConfiguration? previous,
        CancellationToken ct)
    {
        return Task.FromResult(IntegrationStatus.Verified);
    }

    Task OnRemovedAsync(AppContext app, string id, IntegrationConfiguration configured,
        CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    Task<IntegrationStatus> CheckStatusAsync(AppContext app, string id, IntegrationConfiguration configured,
        CancellationToken ct)
    {
        return Task.FromResult(IntegrationStatus.Verified);
    }
}
