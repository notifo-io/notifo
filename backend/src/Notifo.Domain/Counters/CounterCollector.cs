// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure.Timers;

namespace Notifo.Domain.Counters
{
    public sealed class CounterCollector<T> where T : notnull
    {
        private readonly CompletionTimer timer;
        private readonly ICounterStore<T> store;
        private readonly int maxSize;
        private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();
        private ConcurrentDictionary<T, CounterMap> counters = new ConcurrentDictionary<T, CounterMap>();

        public CounterCollector(ICounterStore<T> store, int updateInterval, int maxSize = 20000)
        {
            this.store = store;

            this.maxSize = maxSize;

            timer = new CompletionTimer(updateInterval, StoreAsync, updateInterval);
        }

        public ValueTask AddAsync(T group, CounterMap newCounters)
        {
            readerWriterLock.EnterReadLock();
            try
            {
                counters.AddOrUpdate(group, newCounters, (k, c) => c.IncrementWithLock(newCounters));
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }

            if (counters.Count >= maxSize)
            {
                timer.SkipCurrentDelay();
            }

            return default;
        }

        public Task StopAsync()
        {
            return timer.StopAsync();
        }

        private async Task StoreAsync(CancellationToken ct)
        {
            if (counters.IsEmpty)
            {
                return;
            }

            var current = Interlocked.Exchange(ref counters, new ConcurrentDictionary<T, CounterMap>());

            if (current.IsEmpty)
            {
                return;
            }

            List<(T, CounterMap)> commands;

            readerWriterLock.EnterWriteLock();
            try
            {
                commands = current.Select(x => (x.Key, x.Value)).Where(x => x.Value.Any()).ToList();
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }

            if (commands.Count > 0)
            {
                await store.StoreAsync(commands, ct);
            }
        }
    }
}
