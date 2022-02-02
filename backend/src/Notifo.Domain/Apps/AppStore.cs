// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Counters;
using Notifo.Infrastructure;
using Squidex.Caching;
using Squidex.Log;

namespace Notifo.Domain.Apps
{
    public sealed class AppStore : IAppStore, ICounterTarget, IDisposable
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
        private readonly CounterCollector<string> collector;
        private readonly IAppRepository repository;
        private readonly IServiceProvider services;
        private readonly IReplicatedCache cache;

        public AppStore(IAppRepository repository,
            IServiceProvider services, IReplicatedCache cache, ISemanticLog log)
        {
            this.repository = repository;
            this.services = services;
            this.cache = cache;

            collector = new CounterCollector<string>(repository, log, 5000);
        }

        public void Dispose()
        {
            collector.DisposeAsync().AsTask().Wait();
        }

        public async Task CollectAsync(CounterKey key, CounterMap counters,
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

        public Task<App> UpsertAsync(string? id, ICommand<App> command,
            CancellationToken ct = default)
        {
            Guard.NotNull(command);

            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString();
            }

            return Updater.UpdateRetriedAsync(5, async () =>
            {
                var (app, etag) = await repository.GetAsync(id, ct);

                if (app == null)
                {
                    if (!command.CanCreate)
                    {
                        throw new DomainObjectNotFoundException(id);
                    }

                    app = App.Create(id);
                }

                var newApp = await command.ExecuteAsync(app, services, ct);

                if (newApp == null || ReferenceEquals(app, newApp))
                {
                    return app;
                }

                await repository.UpsertAsync(newApp, etag, ct);

                await DeliverAsync(newApp);

                return newApp;
            });
        }

        private async Task DeliverAsync(App? app)
        {
            CounterMap.Cleanup(app?.Counters);

            if (app != null)
            {
                await cache.AddAsync(app.Id, app, CacheDuration);
            }
        }
    }
}
