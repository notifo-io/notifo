// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Integrations
{
    public sealed record IntegrationDefinition(
        string Type,
        string Title,
        string LogoUrl,
        IReadOnlyList<IntegrationProperty> Properties,
        IReadOnlySet<string> Capabilities)
    {
        public string? Description { get; init; }
    }
}
