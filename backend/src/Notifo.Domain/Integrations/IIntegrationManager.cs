// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Notifo.Domain.Apps;

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Integrations;

public interface IIntegrationManager
{
    IEnumerable<IntegrationDefinition> Definitions { get; }

    IEnumerable<ResolvedIntegration<T>> Resolve<T>(App app, IIntegrationTarget? target);

    bool HasIntegration<T>(App app);

    Task<ProviderImage?> GetImageAsync(string type,
        CancellationToken ct);

    Task OnCallbackAsync(string id, App app, HttpContext httpContext,
        CancellationToken ct = default);

    Task OnRemoveAsync(string id, App app, ConfiguredIntegration configured,
        CancellationToken ct = default);

    Task<IntegrationStatus> OnInstallAsync(string id, App app, ConfiguredIntegration configured, ConfiguredIntegration? previous,
        CancellationToken ct = default);

    ResolvedIntegration<T> Resolve<T>(string id, App app) where T : class;
}

public record struct ResolvedIntegration<T>(string Id, IntegrationContext Context, T System);
