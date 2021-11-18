// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain.Subscriptions
{
    public interface ISubscriptionStore
    {
        IAsyncEnumerable<Subscription> QueryAsync(string appId, TopicId topic, string? userId,
            CancellationToken ct = default);

        Task<IResultList<Subscription>> QueryAsync(string appId, SubscriptionQuery query,
            CancellationToken ct = default);

        Task<Subscription?> GetAsync(string appId, string userId, TopicId prefix,
            CancellationToken ct = default);

        Task<Subscription> UpsertAsync(string appId, string userId, TopicId prefix, ICommand<Subscription> update,
            CancellationToken ct = default);

        Task DeleteAsync(string appId, string userId, TopicId prefix,
            CancellationToken ct = default);

        Task AllowedTopicAddAsync(string appId, string userId, TopicId prefix,
            CancellationToken ct = default);

        Task AllowedTopicRemoveAsync(string appId, string userId, TopicId prefix,
            CancellationToken ct = default);
    }
}
