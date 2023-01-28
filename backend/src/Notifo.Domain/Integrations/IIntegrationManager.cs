// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Notifo.Domain.Apps;

namespace Notifo.Domain.Integrations;

public interface IIntegrationManager
{
    IEnumerable<IntegrationDefinition> Definitions { get; }

    IEnumerable<(string Id, T Integration)> Resolve<T>(App app, IIntegrationTarget? target);

    bool HasIntegration<T>(App app);

    Task OnCallbackAsync<T>(string id, App app, HttpContext httpContext,
        CancellationToken ct = default) where T : class;

    Task OnRemoveAsync(string id, App app, ConfiguredIntegration configured,
        CancellationToken ct = default);

    Task<IntegrationStatus> OnInstallAsync(string id, App app, ConfiguredIntegration configured, ConfiguredIntegration? previous,
        CancellationToken ct = default);

    T? Resolve<T>(string id, App app) where T : class;
}
