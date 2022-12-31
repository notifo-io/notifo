// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.Collections;

namespace Notifo.Domain.Integrations;

public sealed class UserContext
{
    required public string Id { get; init; }

    required public string? EmailAddress { get; init; }

    required public string? PhoneNumber { get; init; }

    required public ReadonlyDictionary<string, string>? Properties { get; init; }
}
