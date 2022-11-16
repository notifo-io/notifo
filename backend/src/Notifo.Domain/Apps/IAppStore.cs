// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Apps;

public interface IAppStore
{
    IAsyncEnumerable<App> QueryAllAsync(
        CancellationToken ct = default);

    Task<List<App>> QueryWithPendingIntegrationsAsync(
        CancellationToken ct = default);

    Task<List<App>> QueryAsync(string contributorId,
        CancellationToken ct = default);

    Task<App?> GetByApiKeyAsync(string apiKey,
        CancellationToken ct = default);

    Task<App?> GetAsync(string id,
        CancellationToken ct = default);

    Task<App?> GetCachedAsync(string id,
        CancellationToken ct = default);
}
