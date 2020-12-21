// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;
using Squidex.Caching;

namespace Notifo.Domain.Users
{
    public sealed class UserStore : IUserStore, ICounterTarget, IDisposable
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
        private readonly IUserRepository repository;
        private readonly IServiceProvider serviceProvider;
        private readonly IReplicatedCache cache;
        private readonly CounterCollector<(string, string)> collector;

        public UserStore(IUserRepository repository, IServiceProvider serviceProvider, IReplicatedCache cache)
        {
            this.repository = repository;
            this.serviceProvider = serviceProvider;
            this.cache = cache;

            collector = new CounterCollector<(string, string)>(repository, 5000);
        }

        public void Dispose()
        {
            collector.StopAsync().Dispose();
        }

        public async Task CollectAsync(CounterKey key, CounterMap counters, CancellationToken ct = default)
        {
            if (key.AppId != null && key.UserId != null)
            {
                await collector.AddAsync((key.AppId, key.UserId), counters);
            }
        }

        public Task<User?> GetCachedAsync(string appId, string id, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNullOrEmpty(id, nameof(id));

            if (cache.TryGetValue($"{appId}_{id}", out var temp) && temp is User user)
            {
                return Task.FromResult<User?>(user);
            }

            return GetAsync(appId, id, ct);
        }

        public async Task<IResultList<User>> QueryAsync(string appId, UserQuery query, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNull(query, nameof(query));

            var users = await repository.QueryAsync(appId, query, ct);

            foreach (var user in users)
            {
                await DeliverAsync(user);
            }

            return users;
        }

        public async Task<User?> GetByApiKeyAsync(string apiKey, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(apiKey, nameof(apiKey));

            var (user, _) = await repository.GetByApiKeyAsync(apiKey, ct);

            await DeliverAsync(user);

            return user;
        }

        public async Task<User?> GetAsync(string appId, string id, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNullOrEmpty(id, nameof(id));

            var (user, _) = await repository.GetAsync(appId, id, ct);

            await DeliverAsync(user);

            return user;
        }

        public Task<User> UpsertAsync(string appId, string? id, ICommand<User> command, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNull(command, nameof(command));

            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString();
            }

            return Updater.UpdateRetriedAsync(5, async () =>
            {
                var (user, etag) = await repository.GetAsync(appId, id, ct);

                if (user == null)
                {
                    if (!command.CanCreate)
                    {
                        throw new DomainObjectNotFoundException(id);
                    }

                    user = User.Create(appId, id);
                }

                await command.ExecuteAsync(user, serviceProvider, ct);

                await repository.UpsertAsync(user, etag, ct);

                await DeliverAsync(user);

                return user;
            });
        }

        public async Task DeleteAsync(string appId, string id, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNullOrEmpty(id, nameof(id));

            await repository.DeleteAsync(appId, id, ct);

            await cache.RemoveAsync($"{appId}_{id}");
        }

        private async Task DeliverAsync(User? user, bool invalidate = false)
        {
            CounterMap.Cleanup(user?.Counters);

            if (user != null)
            {
                await cache.AddAsync(user.ToFullId(), user, CacheDuration, invalidate);
            }
        }
    }
}
