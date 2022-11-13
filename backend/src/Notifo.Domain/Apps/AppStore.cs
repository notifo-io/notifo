// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MediatR;
using Microsoft.Extensions.Logging;
using Notifo.Domain.Counters;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Squidex.Caching;

namespace Notifo.Domain.Apps;

public sealed class AppStore : IAppStore, IRequestHandler<AppCommand, App?>, ICounterTarget, IDisposable
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private readonly CounterCollector<string> collector;
    private readonly IAppRepository repository;
    private readonly IServiceProvider serviceProvider;
    private readonly IReplicatedCache cache;

    public AppStore(IAppRepository repository,
        IServiceProvider serviceProvider, IReplicatedCache cache, ILogger<AppStore> log)
    {
        this.repository = repository;
        this.serviceProvider = serviceProvider;
        this.cache = cache;

        collector = new CounterCollector<string>(repository, log, 5000);
    }

    public void Dispose()
    {
        collector.DisposeAsync().AsTask().Wait();
    }

    public async Task CollectAsync(TrackingKey key, CounterMap counters,
        CancellationToken ct = default)
    {
        if (key.AppId != null)
        {
            await collector.AddAsync(key.AppId, counters, ct);
        }
    }

    public Task<App?> GetCachedAsync(string id,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);

        if (cache.TryGetValue(id, out var temp) && temp is App app)
        {
            return Task.FromResult<App?>(app);
        }

        return GetAsync(id, ct);
    }

    public async Task<List<App>> QueryWithPendingIntegrationsAsync(
        CancellationToken ct = default)
    {
        var apps = await repository.QueryWithPendingIntegrationsAsync(ct);

        foreach (var app in apps)
        {
            await DeliverAsync(app);
        }

        return apps;
    }

    public async Task<List<App>> QueryAsync(string contributorId,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(contributorId);

        var apps = await repository.QueryAsync(contributorId, ct);

        foreach (var app in apps)
        {
            await DeliverAsync(app);
        }

        return apps;
    }

    public async Task<App?> GetAsync(string id,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);

        var (app, _) = await repository.GetAsync(id, ct);

        await DeliverAsync(app);

        return app;
    }

    public async Task<App?> GetByApiKeyAsync(string apiKey,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(apiKey);

        var (app, _) = await repository.GetByApiKeyAsync(apiKey, ct);

        await DeliverAsync(app);

        return app;
    }

    public async Task<App?> Handle(AppCommand command,
        CancellationToken ct)
    {
        Guard.NotNull(command.AppId);

        if (!command.IsUpsert)
        {
            await command.ExecuteAsync(serviceProvider, ct);

            // Invalidates all other copies in the cluster.
            await cache.RemoveAsync(command.AppId, default);
            return null;
        }

        return await Updater.UpdateRetriedAsync(5, async () =>
        {
            var (app, etag) = await repository.GetAsync(command.AppId, ct);

            if (app == null)
            {
                if (!command.CanCreate)
                {
                    throw new DomainObjectNotFoundException(command.AppId);
                }

                app = new App(command.AppId, command.Timestamp);
            }
            else
            {
                app.Integrations ??= ReadonlyDictionary.Empty<string, ConfiguredIntegration>();
            }

            var newApp = await command.ExecuteAsync(app, serviceProvider, ct);

            if (newApp == null || ReferenceEquals(app, newApp))
            {
                await DeliverAsync(app);
                return app;
            }

            newApp = newApp with
            {
                LastUpdate = command.Timestamp
            };

            await repository.UpsertAsync(newApp, etag, ct);

            await DeliverAsync(newApp, true);

            return newApp;
        });
    }

    private async Task DeliverAsync(App? app, bool remove = false)
    {
        if (app == null)
        {
            return;
        }

        CounterMap.Cleanup(app.Counters);

        app.Integrations ??= ReadonlyDictionary.Empty<string, ConfiguredIntegration>();

        if (remove)
        {
            // Invalidates all other copies in the cluster.
            await cache.RemoveAsync(app.Id, default);
        }

        await cache.AddAsync(app.Id, app, CacheDuration, default);
    }
}
