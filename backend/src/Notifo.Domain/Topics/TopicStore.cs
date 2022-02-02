// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Counters;
using Notifo.Infrastructure;
using Squidex.Log;

namespace Notifo.Domain.Topics
{
    public sealed class TopicStore : ITopicStore, ICounterTarget, IDisposable
    {
        private readonly ITopicRepository repository;
        private readonly CounterCollector<(string AppId, string Path)> collector;

        public TopicStore(ITopicRepository repository,
            ISemanticLog log)
        {
            this.repository = repository;

            collector = new CounterCollector<(string AppId, string Path)>(repository, log, 5000);
        }

        public void Dispose()
        {
            collector.DisposeAsync().AsTask().Wait();
        }

        public async Task CollectAsync(CounterKey key, CounterMap counters,
            CancellationToken ct = default)
        {
            if (key.AppId != null && key.Topic != null)
            {
                await collector.AddAsync((key.AppId, key.Topic), counters, ct);
            }
        }

        public async Task<IResultList<Topic>> QueryAsync(string appId, TopicQuery query,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId);
            Guard.NotNull(query);

            var topics = await repository.QueryAsync(appId, query, ct);

            CounterMap.Cleanup(topics.Select(x => x.Counters));

            return topics;
        }
    }
}
