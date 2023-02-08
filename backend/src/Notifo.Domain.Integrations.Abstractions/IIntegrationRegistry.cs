// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;

namespace Notifo.Domain.Integrations;

public interface IIntegrationRegistry
{
    IEnumerable<IIntegration> Integrations { get; }

    bool TryGetIntegration(string type, [MaybeNullWhen(false)] out IIntegration integration);
}
