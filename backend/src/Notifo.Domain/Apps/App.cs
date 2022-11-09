// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Counters;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure.Collections;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Apps;

public sealed record App(string Id, Instant Created)
{
    private static readonly ReadonlyList<string> DefaultLanguages = ReadonlyList.Create("en");

    public string Name { get; init; }

    public string Language => Languages[0];

    public string? ConfirmUrl { get; init; }

    public Instant LastUpdate { get; init; }

    public ReadonlyList<string> Languages { get; init; } = DefaultLanguages;

    public ReadonlyDictionary<string, string> ApiKeys { get; init; } = ReadonlyDictionary.Empty<string, string>();

    public ReadonlyDictionary<string, string> Contributors { get; init; } = ReadonlyDictionary.Empty<string, string>();

    public ReadonlyDictionary<string, ConfiguredIntegration> Integrations { get; set; } = ReadonlyDictionary.Empty<string, ConfiguredIntegration>();

    public CounterMap? Counters { get; init; } = new CounterMap();
}
