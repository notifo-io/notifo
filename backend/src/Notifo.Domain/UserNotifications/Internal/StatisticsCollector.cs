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

        var commands = new List<(TrackingToken Token, DeliveryResult Result)>();

        readerWriterLock.EnterWriteLock();
        try
        {
            while (updateQueue.TryDequeue(out var dequeued))
            {
                commands.Add(dequeued);
            }
        }
        finally
        {
            readerWriterLock.ExitWriteLock();
        }

        if (commands.Count > 0)
        {
            await repository.BatchWriteAsync(commands.ToArray(), clock.GetCurrentInstant(), ct);
        }
    }
}
