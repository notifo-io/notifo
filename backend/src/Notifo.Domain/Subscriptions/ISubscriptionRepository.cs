// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure;

namespace Notifo.Domain.Subscriptions
{
    public interface ISubscriptionRepository
    {
        IAsyncEnumerable<Subscription> QueryAsync(string appId, TopicId topic, string? userId, CancellationToken ct = default);

        Task<IResultList<Subscription>> QueryAsync(string appId, SubscriptionQuery query, CancellationToken ct = default);

        Task<(Subscription? Subscription, string? Etag)> GetAsync(string appId, string userId, TopicId prefix, CancellationToken ct = default);

        Task UpsertAsync(Subscription subscription, string? oldEtag, CancellationToken ct);

        Task DeleteAsync(string appId, string userId, TopicId prefix, CancellationToken ct);

        Task DeletePrefixAsync(string appId, string userId, TopicId prefix, CancellationToken ct = default);
    }
}
