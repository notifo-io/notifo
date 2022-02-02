// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Counters;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Reflection;
using Squidex.Log;

namespace Notifo.Domain.Events
{
    public sealed class EventStore : IEventStore, ICounterTarget, IDisposable
    {
        private readonly CounterCollector<(string, string)> collector;
        private readonly IEventRepository eventRepository;

        public EventStore(IEventRepository eventRepository,
            ISemanticLog log)
        {
            this.eventRepository = eventRepository;

            collector = new CounterCollector<(string, string)>(eventRepository, log, 5000);
        }

        public void Dispose()
        {
            collector.DisposeAsync().AsTask().Wait();
        }

        public async Task CollectAsync(CounterKey key, CounterMap counters,
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

            @event.Counters = new CounterMap();

            return eventRepository.InsertAsync(@event, ct);
        }
    }
}
