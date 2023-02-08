// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;

namespace Notifo.Domain.Integrations;

public sealed class StaticIntegrationRegistry : IIntegrationRegistry
{
    public IEnumerable<IIntegration> Integrations { get; }

    public StaticIntegrationRegistry(IEnumerable<IIntegration> integrations)
    {
        Integrations = integrations;
    }

    public bool TryGetIntegration(string type, [MaybeNullWhen(false)] out IIntegration integration)
    {
        integration = Integrations.FirstOrDefault(x => x.Definition.Type == type);

        return integration != null;
    }
}
