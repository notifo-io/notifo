// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure.Timers;

namespace Notifo.Domain.UserNotifications.Internal
{
    public sealed class StatisticsCollector
    {
        private readonly CompletionTimer timer;
        private readonly IUserNotificationRepository repository;
        private readonly int maxSize;
        private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();
        private ConcurrentDictionary<(Guid Id, string Channel, string Configuration), ChannelSendInfo> updates = new ConcurrentDictionary<(Guid Id, string Channel, string Configuration), ChannelSendInfo>();

        public StatisticsCollector(IUserNotificationRepository repository, int updateInterval, int maxSize = int.MaxValue)
        {
            this.repository = repository;

            this.maxSize = maxSize;

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

            var current = Interlocked.Exchange(ref updates, new ConcurrentDictionary<(Guid Id, string Channel, string Configuration), ChannelSendInfo>());

            if (current.IsEmpty)
            {
                return;
            }

            List<(Guid, string, string, ChannelSendInfo)> commands;

            readerWriterLock.EnterWriteLock();
            try
            {
                commands = current.Select(x => (x.Key.Id, x.Key.Channel, x.Key.Configuration, x.Value)).ToList();
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }

            if (commands.Count > 0)
            {
                await repository.UpdateAsync(commands, ct);
            }
        }
    }
}
