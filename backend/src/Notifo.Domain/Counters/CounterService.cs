// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Counters;

public sealed class CounterService(IEnumerable<ICounterTarget> targets) : ICounterService
{
    public Task CollectAsync(TrackingKey key, CounterMap counters,
        CancellationToken ct = default)
    {
        if (counters.Count == 0)
        {
            return Task.CompletedTask;
        }

        return Task.WhenAll(targets.Select(x => x.CollectAsync(key, counters, ct)));
    }
}
