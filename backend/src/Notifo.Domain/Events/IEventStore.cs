// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain.Events;

public interface IEventStore
{
    Task<IResultList<Event>> QueryAsync(string appId, EventQuery query,
        CancellationToken ct = default);

    Task InsertAsync(EventMessage request,
        CancellationToken ct = default);

    Task<bool> IsPendingAsync(string appId, string id,
        CancellationToken ct = default);

    Task MarkPublishedAsync(string appId, string id,
        CancellationToken ct = default);
}
