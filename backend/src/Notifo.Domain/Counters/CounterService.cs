// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Notifo.Domain.Counters
{
    public sealed class CounterService : ICounterService
    {
        private readonly IEnumerable<ICounterTarget> targets;

        public CounterService(IEnumerable<ICounterTarget> targets)
        {
            this.targets = targets;
        }

        public Task CollectAsync(CounterKey key, CounterMap counters, CancellationToken ct)
        {
            return Task.WhenAll(targets.Select(x => x.CollectAsync(key, counters, ct)));
        }
    }
}
