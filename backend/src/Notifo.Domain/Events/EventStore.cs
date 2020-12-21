// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Domain.Events
{
    public sealed class EventStore : IEventStore, ICounterTarget, IDisposable
    {
        private readonly CounterCollector<(string, string)> collector;
        private readonly IEventRepository eventRepository;

        public EventStore(IEventRepository eventRepository)
        {
            this.eventRepository = eventRepository;

            collector = new CounterCollector<(string, string)>(eventRepository, 1000, 1000);
        }

        public void Dispose()
        {
            collector.StopAsync().Wait();
        }

        public async Task CollectAsync(CounterKey key, CounterMap counters, CancellationToken ct = default)
        {
            if (key.AppId != null && key.EventId != null)
            {
                await collector.AddAsync((key.AppId, key.EventId), counters);
            }
        }

        public async Task<IResultList<Event>> QueryAsync(string appId, EventQuery query, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNull(query, nameof(query));

            var events = await eventRepository.QueryAsync(appId, query, ct);

            CounterMap.Cleanup(events.Select(x => x.Counters));

            return events;
        }

        public Task InsertAsync(EventMessage request, CancellationToken ct = default)
        {
            Guard.NotNull(request, nameof(request));

            var @event = SimpleMapper.Map(request, new Event());

            @event.Counters = new CounterMap();

            return eventRepository.InsertAsync(@event, ct);
        }
    }
}
