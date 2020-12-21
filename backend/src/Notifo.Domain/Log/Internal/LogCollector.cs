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
using NodaTime;
using Notifo.Infrastructure.Timers;

namespace Notifo.Domain.Log.Internal
{
    public sealed class LogCollector
    {
        private readonly CompletionTimer timer;
        private readonly ILogRepository repository;
        private readonly IClock clock;
        private readonly int maxSize;
        private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();
        private ConcurrentDictionary<(string AppId, string Message), int> updates = new ConcurrentDictionary<(string AppId, string Message), int>();

        public LogCollector(ILogRepository repository, IClock clock, int updateInterval, int maxSize = int.MaxValue)
        {
            this.repository = repository;
            this.clock = clock;
            this.maxSize = maxSize;

            timer = new CompletionTimer(updateInterval, StoreAsync, updateInterval);
        }

        public async Task AddAsync(string appId, string message)
        {
            readerWriterLock.EnterReadLock();
            try
            {
                updates.AddOrUpdate((appId, message), 1, (_, value) => value + 1);
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }

            if (updates.Count >= maxSize)
            {
                await StoreAsync(default);
            }
        }

        public Task StopAsync()
        {
            return timer.StopAsync();
        }

        private async Task StoreAsync(CancellationToken ct)
        {
            if (updates.IsEmpty)
            {
                return;
            }

            var current = Interlocked.Exchange(ref updates, new ConcurrentDictionary<(string AppId, string Message), int>());

            if (current.IsEmpty)
            {
                return;
            }

            List<(string AppId, string Message, int Count)> commands;

            readerWriterLock.EnterWriteLock();
            try
            {
                commands = current.Select(x => (x.Key.AppId, x.Key.Message, x.Value)).ToList();
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }

            if (commands.Count > 0)
            {
                var now = clock.GetCurrentInstant();

                await repository.UpdateAsync(commands, now, ct);
            }
        }
    }
}
