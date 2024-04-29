// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Apps;

public sealed class AppAuthScheme
{
    public string Domain { get; init; }

    public string DisplayName { get; init; }

    public string ClientId { get; init; }

    public string ClientSecret { get; init; }

    public string Authority { get; init; }

    public string? SignoutRedirectUrl { get; init; }
}
