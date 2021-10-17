// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Apps;

namespace Notifo.Domain.Integrations
{
    public interface IIntegration
    {
        IntegrationDefinition Definition { get; }

        object? Create(Type serviceType, string id, ConfiguredIntegration configured, IServiceProvider serviceProvider);

        bool CanCreate(Type serviceType, string id, ConfiguredIntegration configured);

        Task OnConfiguredAsync(App app, string id, ConfiguredIntegration configured, ConfiguredIntegration? previous,
            CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        Task OnRemovedAsync(App app, string id, ConfiguredIntegration configured,
            CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        Task CheckStatusAsync(App app, string id, ConfiguredIntegration configured,
            CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}
