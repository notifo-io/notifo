// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;

namespace Notifo.Domain.Integrations;

public sealed class StaticIntegrationRegistry(IEnumerable<IIntegration> integrations) : IIntegrationRegistry
{
    public IEnumerable<IIntegration> Integrations { get; } = integrations;

    public bool TryGetIntegration(string type, [MaybeNullWhen(false)] out IIntegration integration)
    {
        integration = Integrations.FirstOrDefault(x => x.Definition.Type == type);

        return integration != null;
    }
}
