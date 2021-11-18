// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using Notifo.Infrastructure.Timers;

namespace Notifo.Domain.UserNotifications.Internal
{
    public sealed class StatisticsCollector
    {
        private readonly CompletionTimer timer;
        private readonly IUserNotificationRepository repository;
        private readonly int updatesCapacity;
        private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();
        private readonly ConcurrentDictionary<(Guid Id, string Channel, string Configuration), ChannelSendInfo> updates;

        public StatisticsCollector(IUserNotificationRepository repository, int updateInterval, int capacity = 2000)
        {
            this.repository = repository;

            updatesCapacity = capacity;
            updates = new ConcurrentDictionary<(Guid Id, string Channel, string Configuration), ChannelSendInfo>(Environment.ProcessorCount, capacity);

            timer = new CompletionTimer(updateInterval, StoreAsync, updateInterval);
        }

        public async Task AddAsync(Guid id, string channel, string configuration, ChannelSendInfo info)
        {
            readerWriterLock.EnterReadLock();
            try
            {
                updates[(id, channel, configuration)] = info;
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

            List<(Guid, string, string, ChannelSendInfo)> commands;

            readerWriterLock.EnterWriteLock();
            try
            {
                if (updates.IsEmpty)
                {
                    return;
                }

                commands = updates.Select(x => (x.Key.Id, x.Key.Channel, x.Key.Configuration, x.Value)).ToList();
            }
            finally
            {
                updates.Clear();

                readerWriterLock.ExitWriteLock();
            }

            if (commands.Count > 0)
            {
                await repository.BatchWriteAsync(commands, ct);
            }
        }
    }
}
