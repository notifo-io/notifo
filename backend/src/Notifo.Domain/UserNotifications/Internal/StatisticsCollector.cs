// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using NodaTime;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure.Timers;

namespace Notifo.Domain.UserNotifications.Internal;

public sealed class StatisticsCollector
{
    private readonly CompletionTimer timer;
    private readonly IUserNotificationRepository repository;
    private readonly IClock clock;
    private readonly int updatesCapacity;
    private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();
    private readonly ConcurrentQueue<(TrackingToken Token, DeliveryResult Result)> updateQueue;
    private readonly List<(TrackingToken Token, DeliveryResult Result)> failedCommands = [];

    public StatisticsCollector(IUserNotificationRepository repository, IClock clock, int updateInterval, int capacity = 2000)
    {
        this.repository = repository;

        updatesCapacity = capacity;
        updateQueue = new ConcurrentQueue<(TrackingToken Token, DeliveryResult Result)>();

        timer = new CompletionTimer(updateInterval, StoreAsync, updateInterval);

        this.clock = clock;
    }

    public async Task AddAsync(TrackingToken token, DeliveryResult result)
    {
        readerWriterLock.EnterReadLock();
        try
        {
            updateQueue.Enqueue((token, result));
        }
        finally
        {
            readerWriterLock.ExitReadLock();
        }

        if (updateQueue.Count >= updatesCapacity)
        {
            try
            {
                await StoreAsync(default);
            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch (Exception)
            {
                // The updates are kept and written with the next flush, so the caller does not need to fail.
            }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
        }
    }

    public async Task StopAsync()
    {
        await timer.StopAsync();

        // Write the remaining updates, otherwise they would be lost on shutdown.
        await StoreAsync(default);
    }

    private async Task StoreAsync(
        CancellationToken ct)
    {
        if (updateQueue.IsEmpty && failedCommands.Count == 0)
        {
            return;
        }

        var commands = new List<(TrackingToken Token, DeliveryResult Result)>();

        readerWriterLock.EnterWriteLock();
        try
        {
            // Failed updates are older than the queued updates, so they must be written first.
            commands.AddRange(failedCommands);
            failedCommands.Clear();

            while (updateQueue.TryDequeue(out var dequeued))
            {
                commands.Add(dequeued);
            }
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
            await repository.BatchWriteAsync(commands.ToArray(), clock.GetCurrentInstant(), ct);
        }
        catch
        {
            readerWriterLock.EnterWriteLock();
            try
            {
                failedCommands.InsertRange(0, commands);
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }

            throw;
        }
    }
}
