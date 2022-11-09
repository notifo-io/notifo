// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.KeyValueStore;

public interface IKeyValueStore
{
    Task<string?> GetAsync(string key,
        CancellationToken ct = default);

    Task<string?> SetIfNotExistsAsync(string key, string? value,
        CancellationToken ct = default);

    Task<bool> SetAsync(string key, string? value,
        CancellationToken ct = default);

    Task<bool> RemvoveAsync(string key,
        CancellationToken ct = default);
}
