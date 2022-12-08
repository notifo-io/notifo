// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Mediator;
using Squidex.Caching;

namespace Notifo.Domain.Users;

public sealed class UserStore : IUserStore, IRequestHandler<UserCommand, User?>, ICounterTarget, IDisposable
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private readonly IUserRepository repository;
    private readonly IServiceProvider serviceProvider;
    private readonly IReplicatedCache cache;
    private readonly CounterCollector<(string, string)> collector;

    public UserStore(IUserRepository repository,
        IServiceProvider serviceProvider, IReplicatedCache cache, ILogger<UserStore> log)
    {
        this.repository = repository;
        this.serviceProvider = serviceProvider;
        this.cache = cache;

        collector = new CounterCollector<(string, string)>(repository, log, 5000);
    }

    public void Dispose()
    {
        collector.DisposeAsync().AsTask().Wait();
    }

    public async Task CollectAsync(TrackingKey key, CounterMap counters,
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

    public async ValueTask<User?> HandleAsync(UserCommand command,
        CancellationToken ct)
    {
        Guard.NotNullOrEmpty(command.AppId);

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            command.UserId = Guid.NewGuid().ToString();
        }

        if (!command.IsUpsert)
        {
            await command.ExecuteAsync(serviceProvider, ct);

            await cache.RemoveAsync(User.BuildId(command.AppId, command.UserId), default);
            return null;
        }

        return await Updater.UpdateRetriedAsync(5, async () =>
        {
            var (user, etag) = await repository.GetAsync(command.AppId, command.UserId, ct);

            if (user == null)
            {
                if (!command.CanCreate)
                {
                    throw new DomainObjectNotFoundException(command.UserId);
                }

                user = new User(command.AppId, command.UserId, command.Timestamp);
            }

            var newUser = await command.ExecuteAsync(user, serviceProvider, ct);

            if (newUser != null && !ReferenceEquals(newUser, user))
            {
                newUser = newUser with
                {
                    LastUpdate = command.Timestamp
                };

                await repository.UpsertAsync(newUser, etag, ct);
                user = newUser;

                await command.ExecutedAsync(serviceProvider);
            }

            await DeliverAsync(user);

            return user;
        });
    }

    private async Task DeliverAsync(User? user, bool remove = false)
    {
        if (user == null)
        {
            return;
        }

        CounterMap.Cleanup(user.Counters);

        if (remove)
        {
            await cache.RemoveAsync(user.UniqueId, default);
        }

        await cache.AddAsync(user.UniqueId, user, CacheDuration, default);
    }
}
