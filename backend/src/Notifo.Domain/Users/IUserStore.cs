// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain.Users
{
    public interface IUserStore
    {
        IAsyncEnumerable<string> QueryIdsAsync(string appId,
            CancellationToken ct = default);

        Task<IResultList<User>> QueryAsync(string appId, UserQuery query,
            CancellationToken ct = default);

        Task<User?> GetByApiKeyAsync(string apiKey,
            CancellationToken ct = default);

        Task<User?> GetCachedAsync(string appId, string id,
            CancellationToken ct = default);

        Task<User?> GetAsync(string appId, string id,
            CancellationToken ct = default);

        Task<User?> GetByTelegramUsernameAsync(string appId, string username,
            CancellationToken ct = default);

        Task<User> UpsertAsync(string appId, string? id, ICommand<User> update,
            CancellationToken ct = default);

        Task DeleteAsync(string appId, string id,
            CancellationToken ct = default);
    }
}
