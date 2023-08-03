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

    Task<IntegrationStatus> OnConfiguredAsync(IntegrationContext context, IntegrationConfiguration? previous,
        CancellationToken ct)
    {
        return Task.FromResult(IntegrationStatus.Verified);
    }

    Task OnRemovedAsync(IntegrationContext context,
        CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    Task<IntegrationStatus> CheckStatusAsync(IntegrationContext context,
        CancellationToken ct)
    {
        return Task.FromResult(IntegrationStatus.Verified);
    }

    Task<ProviderImage?> GetImageAsync(
        CancellationToken ct)
    {
        return Task.FromResult<ProviderImage?>(null);
    }
}
