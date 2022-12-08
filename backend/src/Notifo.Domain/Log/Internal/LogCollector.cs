// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using NodaTime;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Timers;

namespace Notifo.Domain.Log.Internal;

public sealed class LogCollector
{
    private readonly CompletionTimer timer;
    private readonly ILogRepository repository;
    private readonly IClock clock;
    private readonly int updatesCapacity;
    private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();
    private readonly ConcurrentDictionary<LogWrite, int> updateQueue;

    public Action<IResultList<LogEntry>>? OnNewEntries { get; set; }

    public LogCollector(ILogRepository repository, IClock clock, int updateInterval, int capacity = 2000)
    {
        this.repository = repository;

        updatesCapacity = capacity;
        updateQueue = new ConcurrentDictionary<LogWrite, int>(Environment.ProcessorCount, updatesCapacity);

        timer = new CompletionTimer(updateInterval, StoreAsync, updateInterval);

        this.clock = clock;
    }

    public async Task AddAsync(LogWrite write)
    {
        readerWriterLock.EnterReadLock();
        try
        {
            updateQueue.AddOrUpdate(write, 1, (_, value) => value + 1);
        }
        finally
        {
            readerWriterLock.ExitReadLock();
        }

        if (updateQueue.Count >= updatesCapacity)
        {
            await StoreAsync(default);
        }
    }

    public Task StopAsync()
    {
        return timer.StopAsync();
    }

    private async Task StoreAsync(
        CancellationToken ct)
    {
        if (updateQueue.IsEmpty)
        {
            return;
        }

        var now = clock.GetCurrentInstant();

        List<(LogWrite, int, Instant)> commands;

        readerWriterLock.EnterWriteLock();
        try
        {
            if (updateQueue.IsEmpty)
            {
                return;
            }

            commands = updateQueue.Select(x => (x.Key, x.Value, now)).ToList();
        }
        finally
        {
            updateQueue.Clear();

            readerWriterLock.ExitWriteLock();
        }

        if (commands.Count > 0)
        {
            var newEntries = await repository.BatchWriteAsync(commands, ct);

            if (newEntries.Count > 0)
            {
                OnNewEntries?.Invoke(newEntries);
            }
        }
    }
}
