// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Users;
using Notifo.Infrastructure;

namespace Notifo.Domain.Subscriptions
{
    public sealed class SubscriptionStore : ISubscriptionStore
    {
        private readonly ISubscriptionRepository repository;
        private readonly IUserStore userStore;

        public SubscriptionStore(ISubscriptionRepository repository, IUserStore userStore)
        {
            this.repository = repository;
            this.userStore = userStore;
        }

        public IAsyncEnumerable<Subscription> QueryAsync(string appId, TopicId topic, string? userId, CancellationToken ct = default)
        {
            return repository.QueryAsync(appId, topic, userId, ct);
        }

        public Task<IResultList<Subscription>> QueryAsync(string appId, SubscriptionQuery query, CancellationToken ct = default)
        {
            return repository.QueryAsync(appId, query, ct);
        }

        public Task<Subscription?> GetAsync(string appId, string userId, TopicId prefix, CancellationToken ct = default)
        {
            return repository.GetAsync(appId, userId, prefix, ct);
        }

        public Task<Subscription> SubscribeAsync(string appId, SubscriptionUpdate update, CancellationToken ct = default)
        {
            return repository.SubscribeAsync(appId, update, ct);
        }

        public async Task SubscribeWhenNotFoundAsync(string appId, string userId, TopicId prefix, CancellationToken ct = default)
        {
            await CheckWhitelistAsync(appId, userId, prefix, ct);

            await repository.SubscribeAsync(appId, new SubscriptionUpdate { UserId = userId, TopicPrefix = prefix }, ct);
        }

        public async Task SubscribeWhenNotFoundAsync(string appId, SubscriptionUpdate update, CancellationToken ct = default)
        {
            await CheckWhitelistAsync(appId, update.UserId, update.TopicPrefix, ct);

            await repository.SubscribeAsync(appId, update, ct);
        }

        private async Task CheckWhitelistAsync(string appId, string userId, TopicId topic, CancellationToken ct = default)
        {
            var user = await userStore.GetCachedAsync(appId, userId, ct);

            if (user == null)
            {
                throw new DomainObjectNotFoundException(userId);
            }

            if (user.AllowedTopics == null)
            {
                return;
            }

            if (user.AllowedTopics.Count == 0 && !user.RequiresWhitelistedTopics)
            {
                return;
            }

            if (!user.AllowedTopics.Any(x => topic.StartsWith(x)))
            {
                throw new DomainForbiddenException("Topic is not whitelisted.");
            }
        }

        public async Task AddAllowedTopicAsync(string appId, string userId, TopicId prefix, CancellationToken ct = default)
        {
            var command = new AddUserAllowedTopic
            {
                Prefix = prefix
            };

            await userStore.UpsertAsync(appId, userId, command, ct);
        }

        public async Task RemoveAllowedTopicAsync(string appId, string userId, TopicId prefix, CancellationToken ct = default)
        {
            var command = new RemoveUserAllowedTopic
            {
                Prefix = prefix
            };

            await userStore.UpsertAsync(appId, userId, command, ct);

            await repository.UnsubscribeByPrefixAsync(appId, userId, prefix, ct);
        }

        public Task UnsubscribeAsync(string appId, string userId, TopicId prefix, CancellationToken ct = default)
        {
            return repository.UnsubscribeAsync(appId, userId, prefix, ct);
        }
    }
}
