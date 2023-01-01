// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;

namespace Notifo.Domain.Integrations;

public interface IIntegrationManager
{
    IEnumerable<IntegrationDefinition> Definitions { get; }

    bool IsConfigured<T>(App app, IIntegrationTarget? target = null);

    Task ValidateAsync(ConfiguredIntegration configured,
        CancellationToken ct = default);

    Task HandleRemovedAsync(string id, App app, ConfiguredIntegration configured,
        CancellationToken ct = default);

    Task<IntegrationStatus> HandleConfiguredAsync(string id, App app, ConfiguredIntegration configured, ConfiguredIntegration? previous,
        CancellationToken ct = default);

    T? Resolve<T>(string id, App app, IIntegrationTarget? target = null) where T : class;

    IEnumerable<(string Id, T Target)> Resolve<T>(App app, IIntegrationTarget? target = null) where T : class;
}
