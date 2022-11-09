// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.Collections;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Integrations;

public sealed record ConfiguredIntegration(string Type, ReadonlyDictionary<string, string> Properties)
{
    public bool Enabled { get; init; }

    public bool? Test { get; init; }

    public int Priority { get; init; }

    public string? Condition { get; init; }

    public IntegrationStatus Status { get; set; }
}
