// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Core;

namespace Notifo.Identity;

public sealed class ApplicationManager<T>(
    IOptionsMonitor<OpenIddictCoreOptions> options,
    IOpenIddictApplicationCache<T> cache,
    IOpenIddictApplicationStoreResolver resolver,
    ILogger<OpenIddictApplicationManager<T>> logger)
    : OpenIddictApplicationManager<T>(cache, logger, options, resolver) where T : class
{
    protected override ValueTask<bool> ValidateClientSecretAsync(string secret, string comparand,
        CancellationToken cancellationToken = default)
    {
        return new ValueTask<bool>(string.Equals(secret, comparand, StringComparison.Ordinal));
    }
}
