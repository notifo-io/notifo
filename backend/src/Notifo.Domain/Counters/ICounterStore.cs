// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Counters;

public interface ICounterStore<T> where T : notnull
{
    Task BatchWriteAsync(List<(T Key, CounterMap Counters)> counters,
        CancellationToken ct);
}
