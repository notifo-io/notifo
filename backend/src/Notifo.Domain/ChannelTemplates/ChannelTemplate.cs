// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Infrastructure.Collections;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.ChannelTemplates;

public sealed record ChannelTemplate<T>(string AppId, string Id, Instant Created)
{
    public string? Name { get; init; }

    public string? Kind { get; init; }

    public bool Primary { get; init; }

    public Instant LastUpdate { get; init; }

    public ReadonlyDictionary<string, T> Languages { get; init; } = ReadonlyDictionary.Empty<string, T>();
}
