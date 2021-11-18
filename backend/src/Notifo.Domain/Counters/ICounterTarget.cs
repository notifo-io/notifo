// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Counters
{
    public interface ICounterTarget
    {
        Task CollectAsync(CounterKey key, CounterMap counters,
            CancellationToken ct = default);
    }
}
