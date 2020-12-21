// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;

namespace Notifo.Domain.Events
{
    public interface IEventRepository : ICounterStore<(string AppId, string EventId)>
    {
        Task<IResultList<Event>> QueryAsync(string appId, EventQuery query, CancellationToken ct);

        Task InsertAsync(Event @event, CancellationToken ct);
    }
}
