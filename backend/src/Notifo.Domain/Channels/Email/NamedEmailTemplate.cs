// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Domain.Channels.Email;

public sealed class NamedEmailTemplate
{
    public string? Name { get; init; }

    public bool Primary { get; init; }

    public Instant LastUpdate { get; init; }

    public Dictionary<string, EmailTemplate> Languages { get; init; } = new Dictionary<string, EmailTemplate>();
}
