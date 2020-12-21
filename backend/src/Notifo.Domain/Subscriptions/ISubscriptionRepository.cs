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

        Task<Subscription?> GetAsync(string appId, string userId, TopicId prefix, CancellationToken ct = default);

        Task<Subscription> SubscribeAsync(string appId, SubscriptionUpdate update, CancellationToken ct = default);

        Task SubscribeWhenNotFoundAsync(string appId, SubscriptionUpdate update, CancellationToken ct = default);

        Task UnsubscribeAsync(string appId, string userId, TopicId prefix, CancellationToken ct = default);

        Task UnsubscribeByPrefixAsync(string appId, string userId, TopicId prefix, CancellationToken ct = default);
    }
}
