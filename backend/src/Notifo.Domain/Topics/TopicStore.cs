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

namespace Notifo.Domain.Topics;

public sealed class TopicStore : ITopicStore, IRequestHandler<TopicCommand, Topic?>, ICounterTarget, IDisposable
{
    private readonly ITopicRepository repository;
    private readonly IServiceProvider serviceProvider;
    private readonly CounterCollector<(string AppId, string Path)> collector;

    public TopicStore(ITopicRepository repository,
        IServiceProvider serviceProvider, ILogger<TopicStore> log)
    {
        this.repository = repository;
        this.serviceProvider = serviceProvider;

        collector = new CounterCollector<(string AppId, string Path)>(repository, log, 5000);
    }

    public void Dispose()
    {
        collector.DisposeAsync().AsTask().Wait();
    }

    public async Task CollectAsync(TrackingKey key, CounterMap counters,
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

    public async ValueTask<Topic?> HandleAsync(TopicCommand command,
        CancellationToken ct)
    {
        Guard.NotNullOrEmpty(command.AppId);
        Guard.NotNullOrEmpty(command.Path);

        if (!command.IsUpsert)
        {
            await command.ExecuteAsync(serviceProvider, ct);
            return null;
        }

        return await Updater.UpdateRetriedAsync(5, async () =>
        {
            var (topic, etag) = await repository.GetAsync(command.AppId, command.Path, ct);

            if (topic == null)
            {
                if (!command.CanCreate)
                {
                    throw new DomainObjectNotFoundException(command.Path);
                }

                topic = new Topic(command.AppId, command.Path, command.Timestamp);
            }

            var newTopic = await command.ExecuteAsync(topic, serviceProvider, ct);

            if (newTopic != null && !ReferenceEquals(newTopic, topic))
            {
                newTopic = newTopic with
                {
                    LastUpdate = command.Timestamp
                };

                await repository.UpsertAsync(newTopic, etag, ct);
                topic = newTopic;

                await command.ExecutedAsync(serviceProvider);
            }

            return topic;
        });
    }
}
