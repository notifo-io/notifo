// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Counters;
using Notifo.Infrastructure;

namespace Notifo.Domain.Events
{
    public interface IEventRepository : ICounterStore<(string AppId, string EventId)>
    {
        Task<IResultList<Event>> QueryAsync(string appId, EventQuery query,
            CancellationToken ct = default);

        Task InsertAsync(Event @event,
            CancellationToken ct = default);
    }
}
