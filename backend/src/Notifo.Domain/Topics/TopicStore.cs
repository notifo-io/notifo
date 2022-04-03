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

namespace Notifo.Domain.Topics
{
    public sealed class TopicStore : ITopicStore, ICounterTarget, IDisposable
    {
        private readonly ITopicRepository repository;
        private readonly IServiceProvider services;
        private readonly IClock clock;
        private readonly CounterCollector<(string AppId, string Path)> collector;

        public TopicStore(ITopicRepository repository,
            IServiceProvider services, IClock clock, ILogger<TopicStore> log)
        {
            this.repository = repository;
            this.services = services;
            this.clock = clock;

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

        public Task<Topic> UpsertAsync(string appId, TopicId path, ICommand<Topic> command,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId);
            Guard.NotNullOrEmpty(path);
            Guard.NotNull(command);

            return Updater.UpdateRetriedAsync(5, async () =>
            {
                var (topic, etag) = await repository.GetAsync(appId, path, ct);

                if (topic == null)
                {
                    if (!command.CanCreate)
                    {
                        throw new DomainObjectNotFoundException(path);
                    }

                    topic = new Topic(appId, path, clock.GetCurrentInstant());
                }

                var newTopic = await command.ExecuteAsync(topic, services, ct);

                if (newTopic == null || ReferenceEquals(newTopic, topic))
                {
                    return topic;
                }

                newTopic = newTopic with
                {
                    LastUpdate = clock.GetCurrentInstant()
                };

                await repository.UpsertAsync(newTopic, etag, ct);

                return newTopic;
            });
        }

        public Task DeleteAsync(string appId, TopicId path,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId);
            Guard.NotNullOrEmpty(path);

            return repository.DeleteAsync(appId, path, ct);
        }
    }
}
