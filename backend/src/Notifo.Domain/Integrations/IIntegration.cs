// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Notifo.Domain.Integrations
{
    public interface IIntegration
    {
        IntegrationDefinition Definition { get; }

        object? Create(Type implementationType, ConfiguredIntegration configured);

        bool CanCreate<T>(ConfiguredIntegration configured);

        Task OnConfiguredAsync(ConfiguredIntegration configured, ConfiguredIntegration? previous)
        {
            return Task.CompletedTask;
        }

        Task CheckStatusAsync(ConfiguredIntegration configured)
        {
            return Task.CompletedTask;
        }
    }
}
