// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;

namespace Notifo.Domain.Users
{
    public interface IUserRepository : ICounterStore<(string AppId, string UserId)>
    {
        Task<IResultList<User>> QueryAsync(string appId, UserQuery query, CancellationToken ct);

        Task<(User? User, string? Etag)> GetByApiKeyAsync(string apiKey, CancellationToken ct);

        Task<(User? User, string? Etag)> GetAsync(string appId, string id, CancellationToken ct);

        Task UpsertAsync(User user, string? oldEtag, CancellationToken ct);

        Task DeleteAsync(string appId, string id, CancellationToken ct);
    }
}
