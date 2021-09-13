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
using Notifo.Infrastructure;

namespace Notifo.Domain.Users
{
    public interface IUserRepository : ICounterStore<(string AppId, string UserId)>
    {
        IAsyncEnumerable<string> QueryIdsAsync(string appId,
            CancellationToken ct = default);

        Task<IResultList<User>> QueryAsync(string appId, UserQuery query,
            CancellationToken ct = default);

        Task<(User? User, string? Etag)> GetByApiKeyAsync(string apiKey,
            CancellationToken ct = default);

        Task<(User? User, string? Etag)> GetAsync(string appId, string id,
            CancellationToken ct = default);

        Task UpsertAsync(User user, string? oldEtag = null,
            CancellationToken ct = default);

        Task DeleteAsync(string appId, string id,
            CancellationToken ct = default);
    }
}
