// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;

namespace Notifo.Domain.Counters
{
    public interface ICounterService
    {
        Task CollectAsync(CounterKey key, CounterMap counters, CancellationToken ct = default);
    }
}
