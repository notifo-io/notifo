// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Identity.Dynamic;

public interface IConfigurationStore<T> where T : class
{
    Task SetAsync(string key, T value, TimeSpan ttl,
        CancellationToken ct = default);

    Task<T?> GetAsync(string key,
        CancellationToken ct = default);
}
