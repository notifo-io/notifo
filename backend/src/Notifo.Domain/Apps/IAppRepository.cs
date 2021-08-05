// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Counters;

namespace Notifo.Domain.Apps
{
    public interface IAppRepository : ICounterStore<string>
    {
        Task<List<App>> QueryWithPendingIntegrationsAsync(CancellationToken ct);

        Task<List<App>> QueryAsync(string contributorId,
            CancellationToken ct);

        Task<(App? App, string? Etag)> GetByApiKeyAsync(string apiKey,
            CancellationToken ct);

        Task<(App? App, string? Etag)> GetAsync(string id,
            CancellationToken ct);

        Task UpsertAsync(App app, string? oldEtag,
            CancellationToken ct);

        Task DeleteAsync(string id,
            CancellationToken ct);
    }
}
