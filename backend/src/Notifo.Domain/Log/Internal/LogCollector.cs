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
    private readonly ConcurrentDictionary<(string AppId, int EventCode, string Text, string System), int> updateQueue;

    public Action<IResultList<LogEntry>>? OnNewEntries { get; set; }

    public LogCollector(ILogRepository repository, IClock clock, int updateInterval, int capacity = 2000)
    {
        this.repository = repository;

        updatesCapacity = capacity;
        updateQueue = new ConcurrentDictionary<(string AppId, int EventCode, string Text, string System), int>(Environment.ProcessorCount, updatesCapacity);

        timer = new CompletionTimer(updateInterval, StoreAsync, updateInterval);

        this.clock = clock;
    }

    public async Task AddAsync(string appId, int eventCode, string text, string system)
    {
        readerWriterLock.EnterReadLock();
        try
        {
            updateQueue.AddOrUpdate((appId, eventCode, text, system), 1, (_, value) => value + 1);
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

        List<(string AppId, int EventCode, string Message, string Reason, int Count)> commands;

        readerWriterLock.EnterWriteLock();
        try
        {
            if (updateQueue.IsEmpty)
            {
                return;
            }

            commands = updateQueue.Select(x => (x.Key.AppId, x.Key.EventCode, x.Key.Text, x.Key.System, x.Value)).ToList();
        }
        finally
        {
            updateQueue.Clear();

            readerWriterLock.ExitWriteLock();
        }

        if (commands.Count > 0)
        {
            var now = clock.GetCurrentInstant();

            var newEntries = await repository.BatchWriteAsync(commands, now, ct);

            if (newEntries.Count > 0)
            {
                OnNewEntries?.Invoke(newEntries);
            }
        }
    }
}
