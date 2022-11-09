// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using NodaTime;
using Notifo.Infrastructure.Timers;

namespace Notifo.Domain.Log.Internal;

public sealed class LogCollector
{
    private readonly CompletionTimer timer;
    private readonly ILogRepository repository;
    private readonly IClock clock;
    private readonly int updatesCapacity;
    private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();
    private readonly ConcurrentDictionary<(string AppId, string Message), int> updates;

    public LogCollector(ILogRepository repository, IClock clock, int updateInterval, int capacity = 2000)
    {
        this.repository = repository;

        this.clock = clock;

        updatesCapacity = capacity;
        updates = CreateUpdates();

        timer = new CompletionTimer(updateInterval, StoreAsync, updateInterval);
    }

    private ConcurrentDictionary<(string AppId, string Message), int> CreateUpdates()
    {
        return new ConcurrentDictionary<(string AppId, string Message), int>(Environment.ProcessorCount, updatesCapacity);
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

        if (updates.Count >= updatesCapacity)
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
        if (updates.IsEmpty)
        {
            return;
        }

        List<(string AppId, string Message, int Count)> commands;

        readerWriterLock.EnterWriteLock();
        try
        {
            if (updates.IsEmpty)
            {
                return;
            }

            commands = updates.Select(x => (x.Key.AppId, x.Key.Message, x.Value)).ToList();
        }
        finally
        {
            updates.Clear();

            readerWriterLock.ExitWriteLock();
        }

        if (commands.Count > 0)
        {
            var now = clock.GetCurrentInstant();

            await repository.MatchWriteAsync(commands, now, ct);
        }
    }
}
