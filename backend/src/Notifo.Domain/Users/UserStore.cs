// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;
using Squidex.Caching;

namespace Notifo.Domain.Users
{
    public sealed class UserStore : IUserStore, ICounterTarget, IDisposable
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
        private readonly IUserRepository repository;
        private readonly IServiceProvider services;
        private readonly IReplicatedCache cache;
        private readonly IClock clock;
        private readonly CounterCollector<(string, string)> collector;

        public UserStore(IUserRepository repository,
            IServiceProvider services, IReplicatedCache cache, IClock clock, ILogger<UserStore> log)
        {
            this.repository = repository;
            this.services = services;
            this.cache = cache;
            this.clock = clock;

            collector = new CounterCollector<(string, string)>(repository, log, 5000);
        }

        public void Dispose()
        {
            collector.DisposeAsync().AsTask().Wait();
        }

        public async Task CollectAsync(CounterKey key, CounterMap counters,
            CancellationToken ct = default)
        {
            if (key.AppId != null && key.UserId != null)
            {
                await collector.AddAsync((key.AppId, key.UserId), counters, ct);
            }
        }

        public IAsyncEnumerable<string> QueryIdsAsync(string appId,
            CancellationToken ct = default)
        {
            return repository.QueryIdsAsync(appId, ct);
        }

        public Task<User?> GetCachedAsync(string appId, string id,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId);
            Guard.NotNullOrEmpty(id);

            if (cache.TryGetValue($"{appId}_{id}", out var temp) && temp is User user)
            {
                return Task.FromResult<User?>(user);
            }

            return GetAsync(appId, id, ct);
        }

        public async Task<IResultList<User>> QueryAsync(string appId, UserQuery query,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId);
            Guard.NotNull(query);

            var users = await repository.QueryAsync(appId, query, ct);

            foreach (var user in users)
            {
                await DeliverAsync(user);
            }

            return users;
        }

        public async Task<User?> GetByApiKeyAsync(string apiKey,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(apiKey);

            var (user, _) = await repository.GetByApiKeyAsync(apiKey, ct);

            await DeliverAsync(user);

            return user;
        }

        public async Task<User?> GetAsync(string appId, string id,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId);
            Guard.NotNullOrEmpty(id);

            var (user, _) = await repository.GetAsync(appId, id, ct);

            await DeliverAsync(user);

            return user;
        }

        public async Task<User?> GetByPropertyAsync(string appId, string key, string value,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId);
            Guard.NotNullOrEmpty(key);

            var (user, _) = await repository.GetByPropertyAsync(appId, key, value, ct);

            await DeliverAsync(user);

            return user;
        }

        public Task<User> UpsertAsync(string appId, string? id, ICommand<User> command,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId);
            Guard.NotNull(command);

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

                    user = new User(appId, id, clock.GetCurrentInstant());
                }

                var newUser = await command.ExecuteAsync(user, services, ct);

                if (newUser == null || ReferenceEquals(newUser, user))
                {
                    return user;
                }

                newUser = newUser with
                {
                    LastUpdate = clock.GetCurrentInstant()
                };

                await repository.UpsertAsync(newUser, etag, ct);

                await DeliverAsync(newUser);

                return newUser;
            });
        }

        public async Task DeleteAsync(string appId, string id,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId);
            Guard.NotNullOrEmpty(id);

            await repository.DeleteAsync(appId, id, ct);

            await cache.RemoveAsync($"{appId}_{id}");
        }

        private async Task DeliverAsync(User? user)
        {
            CounterMap.Cleanup(user?.Counters);

            if (user != null)
            {
                await cache.AddAsync(user.UniqueId, user, CacheDuration);
            }
        }
    }
}
