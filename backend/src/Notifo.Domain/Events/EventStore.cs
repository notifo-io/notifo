// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Domain.Events;

public sealed class EventStore(
    IEventRepository eventRepository,
    ILogger<EventStore> log)
    :  IEventStore, ICounterTarget, IDisposable
{
    private readonly CounterCollector<(string, string)> collector = new CounterCollector<(string, string)>(eventRepository, log, 5000);

    public void Dispose()
    {
        collector.DisposeAsync().AsTask().Wait();
    }

    public async Task CollectAsync(TrackingKey key, CounterMap counters,
        CancellationToken ct = default)
    {
        if (key.AppId != null && key.EventId != null)
        {
            await collector.AddAsync((key.AppId, key.EventId), counters, ct);
        }
    }

    public async Task<IResultList<Event>> QueryAsync(string appId, EventQuery query,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(appId);
        Guard.NotNull(query);

        var events = await eventRepository.QueryAsync(appId, query, ct);

        CounterMap.Cleanup(events.Select(x => x.Counters));

        return events;
    }

    public Task InsertAsync(EventMessage request,
        CancellationToken ct = default)
    {
        Guard.NotNull(request);

        var @event = SimpleMapper.Map(request, new Event());

        @event.Counters = [];

        return eventRepository.InsertAsync(@event, ct);
    }
}
