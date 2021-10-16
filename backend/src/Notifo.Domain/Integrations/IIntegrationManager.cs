// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Notifo.Domain.Apps;

namespace Notifo.Domain.Integrations
{
    public interface IIntegrationManager
    {
        IEnumerable<IntegrationDefinition> Definitions { get; }

        bool IsConfigured<T>(App app, bool test);

        Task ValidateAsync(ConfiguredIntegration configured);

        Task HandleConfiguredAsync(ConfiguredIntegration configured, ConfiguredIntegration? previous);

        T? Resolve<T>(string id, App app, bool test) where T : class;

        IEnumerable<(string Id, T Target)> Resolve<T>(App app, bool test) where T : class;
    }
}
