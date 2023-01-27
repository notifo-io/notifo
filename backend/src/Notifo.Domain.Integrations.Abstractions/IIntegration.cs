// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;

namespace Notifo.Domain.Integrations;

public interface IIntegration
{
    IntegrationDefinition Definition { get; }

    bool CanCreate(Type serviceType, IntegrationContext context);

    object? Create(Type serviceType, IntegrationContext context, IServiceProvider serviceProvider);

    Task HandleWebhookAsync(Type serviceType, IntegrationContext context, HttpContext httpContext, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        return Task.CompletedTask;
    }

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
}
