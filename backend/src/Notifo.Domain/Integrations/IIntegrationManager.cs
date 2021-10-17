// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Apps;

namespace Notifo.Domain.Integrations
{
    public interface IIntegrationManager
    {
        IEnumerable<IntegrationDefinition> Definitions { get; }

        bool IsConfigured<T>(App app, bool? test = null);

        Task ValidateAsync(ConfiguredIntegration configured,
            CancellationToken ct = default);

        Task HandleConfiguredAsync(string id, App app, ConfiguredIntegration configured, ConfiguredIntegration? previous,
            CancellationToken ct = default);

        Task HandleRemovedAsync(string id, App app, ConfiguredIntegration configured,
            CancellationToken ct = default);

        T? Resolve<T>(string id, App app, bool? test = null) where T : class;

        IEnumerable<(string Id, T Target)> Resolve<T>(App app, bool? test = null) where T : class;
    }
}
