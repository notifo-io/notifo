// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure;

namespace Notifo.Domain.Events
{
    public interface IEventStore
    {
        Task<IResultList<Event>> QueryAsync(string appId, EventQuery query,
            CancellationToken ct = default);

        Task InsertAsync(EventMessage request,
            CancellationToken ct = default);
    }
}
