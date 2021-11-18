// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Counters;
using Notifo.Infrastructure;

namespace Notifo.Domain.Topics
{
    public sealed class TopicStore : ITopicStore, ICounterTarget, IDisposable
    {
        private readonly ITopicRepository repository;
        private readonly CounterCollector<(string AppId, string Path)> collector;

        public TopicStore(ITopicRepository repository)
        {
            this.repository = repository;

            collector = new CounterCollector<(string AppId, string Path)>(repository, 5000);
        }

        public void Dispose()
        {
            collector.StopAsync().Wait();
        }

        public async Task CollectAsync(CounterKey key, CounterMap counters,
            CancellationToken ct = default)
        {
            if (key.AppId != null && key.Topic != null)
            {
                await collector.AddAsync((key.AppId, key.Topic), counters);
            }
        }

        public async Task<IResultList<Topic>> QueryAsync(string appId, TopicQuery query,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNull(query, nameof(query));

            var topics = await repository.QueryAsync(appId, query, ct);

            CounterMap.Cleanup(topics.Select(x => x.Counters));

            return topics;
        }
    }
}
