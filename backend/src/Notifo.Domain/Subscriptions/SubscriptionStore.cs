// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Users;
using Notifo.Infrastructure;

namespace Notifo.Domain.Subscriptions
{
    public sealed class SubscriptionStore : ISubscriptionStore
    {
        private readonly ISubscriptionRepository repository;
        private readonly IServiceProvider serviceProvider;
        private readonly IUserStore userStore;

        public SubscriptionStore(ISubscriptionRepository repository,
            IServiceProvider serviceProvider, IUserStore userStore)
        {
            this.repository = repository;
            this.serviceProvider = serviceProvider;
            this.userStore = userStore;
        }

        public IAsyncEnumerable<Subscription> QueryAsync(string appId, TopicId topic, string? userId,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId);

            return repository.QueryAsync(appId, topic, userId, ct);
        }

        public Task<IResultList<Subscription>> QueryAsync(string appId, SubscriptionQuery query,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId);

            return repository.QueryAsync(appId, query, ct);
        }

        public async Task<Subscription?> GetAsync(string appId, string userId, TopicId prefix,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId);

            var (subscription, _) = await repository.GetAsync(appId, userId, prefix, ct);

            return subscription;
        }

        public Task<Subscription> UpsertAsync(string appId, string userId, TopicId prefix, ICommand<Subscription> command,
            CancellationToken ct = default)
        {
            Guard.NotNull(command);

            return Updater.UpdateRetriedAsync(5, async () =>
            {
                var (subscription, etag) = await repository.GetAsync(appId, userId, prefix, ct);

                if (subscription == null)
                {
                    if (!command.CanCreate)
                    {
                        throw new DomainObjectNotFoundException(prefix.ToString());
                    }

                    subscription = Subscription.Create(appId, userId, prefix);
                }

                var newSubscription = await command.ExecuteAsync(subscription, serviceProvider, ct);

                if (newSubscription == null || ReferenceEquals(subscription, newSubscription))
                {
                    return subscription;
                }

                await repository.UpsertAsync(newSubscription, etag, ct);

                return newSubscription;
            });
        }

        public async Task AllowedTopicAddAsync(string appId, string userId, TopicId prefix,
            CancellationToken ct = default)
        {
            var command = new AddUserAllowedTopic
            {
                Prefix = prefix
            };

            await userStore.UpsertAsync(appId, userId, command, ct);

            await repository.DeletePrefixAsync(appId, userId, prefix, ct);
        }

        public async Task AllowedTopicRemoveAsync(string appId, string userId, TopicId prefix,
            CancellationToken ct = default)
        {
            var command = new RemoveUserAllowedTopic
            {
                Prefix = prefix
            };

            await userStore.UpsertAsync(appId, userId, command, ct);

            await repository.DeletePrefixAsync(appId, userId, prefix, ct);
        }

        public Task DeleteAsync(string appId, string userId, TopicId prefix,
            CancellationToken ct = default)
        {
            return repository.DeleteAsync(appId, userId, prefix, ct);
        }
    }
}
