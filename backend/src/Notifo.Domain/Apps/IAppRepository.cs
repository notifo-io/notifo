// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Counters;

namespace Notifo.Domain.Apps;

public interface IAppRepository : ICounterStore<string>
{
    IAsyncEnumerable<App> QueryAllAsync(
        CancellationToken ct = default);

    Task<List<App>> QueryWithPendingIntegrationsAsync(
        CancellationToken ct = default);

    Task<List<App>> QueryAsync(string contributorId,
        CancellationToken ct = default);

    Task<(App? App, string? Etag)> GetByApiKeyAsync(string apiKey,
        CancellationToken ct = default);

    Task<(App? App, string? Etag)> GetAsync(string id,
        CancellationToken ct = default);

    Task UpsertAsync(App app, string? oldEtag = null,
        CancellationToken ct = default);

    Task DeleteAsync(string id,
        CancellationToken ct = default);
}
