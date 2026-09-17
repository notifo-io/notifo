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
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            try
            {
                await StoreAsync(default);
            }
            catch (Exception)
            {
                // The entries are kept and written with the next flush, so the caller does not need to fail.
            }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
        }
    }

    public async Task StopAsync()
    {
        await timer.StopAsync();

        // Write the remaining entries, otherwise they would be lost on shutdown.
        await StoreAsync(default);
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

            commands = [];

            // Use a normal loop to avoid the allocations of the closure.
            foreach (var (key, value) in updateQueue)
            {
                commands.Add((key, value, now));
            }

            updateQueue.Clear();
        }
        finally
        {
            readerWriterLock.ExitWriteLock();
        }

        if (commands.Count == 0)
        {
            return;
        }

        try
        {
            var newEntries = await repository.BatchWriteAsync(commands, ct);

            if (newEntries.Count > 0)
            {
                OnNewEntries?.Invoke(newEntries);
            }
        }
        catch
        {
            readerWriterLock.EnterWriteLock();
            try
            {
                // Add the entries again, so that they are written with the next flush.
                foreach (var (write, count, _) in commands)
                {
                    updateQueue.AddOrUpdate(write, count, (_, value) => value + count);
                }
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }

            throw;
        }
    }
}
