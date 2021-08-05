// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Notifo.Domain.Counters
{
    public interface ICounterStore<T> where T : notnull
    {
        Task BatchWriteAsync(List<(T Key, CounterMap Counters)> counters,
            CancellationToken ct);
    }
}
