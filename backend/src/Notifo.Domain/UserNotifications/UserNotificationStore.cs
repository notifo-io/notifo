// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Notifo.Domain.Counters;
using Notifo.Domain.UserEvents;
using Notifo.Domain.UserNotifications.Internal;
using Notifo.Infrastructure;

namespace Notifo.Domain.UserNotifications
{
    public sealed class UserNotificationStore : IUserNotificationStore, IDisposable
    {
        private readonly StatisticsCollector collector;
        private readonly IUserNotificationRepository repository;
        private readonly ICounterService counters;
        private readonly IClock clock;

        public UserNotificationStore(IUserNotificationRepository repository, ICounterService counters, IClock clock)
        {
            this.repository = repository;
            this.counters = counters;
            this.clock = clock;

            collector = new StatisticsCollector(repository, 5000);
        }

        public void Dispose()
        {
            collector.StopAsync();
        }

        public Task<IResultList<UserNotification>> QueryAsync(string appId, string userId, UserNotificationQuery query, CancellationToken ct)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNullOrEmpty(userId, nameof(userId));
            Guard.NotNull(query, nameof(query));

            return repository.QueryAsync(appId, userId, query, ct);
        }

        public Task<UserNotification?> FindAsync(Guid id)
        {
            return repository.FindAsync(id);
        }

        public Task DeleteAsync(Guid id, CancellationToken ct)
        {
            return repository.DeleteAsync(id, ct);
        }

        public Task<bool> IsConfirmedOrHandled(Guid id, string channel, string configuration)
        {
            return repository.IsConfirmedOrHandled(id, channel, configuration);
        }

        public Task<UserNotification?> TrackConfirmedAsync(Guid id, string? channel = null)
        {
            Guard.NotDefault(id, nameof(id));

            var handle = CreateHandle(channel);

            return repository.TrackConfirmedAsync(id, handle);
        }

        public Task TrackSeenAsync(IEnumerable<Guid> ids, string? channel = null)
        {
            Guard.NotNull(ids, nameof(ids));

            var handle = CreateHandle(channel);

            return repository.TrackSeenAsync(ids, handle);
        }

        public Task TrackAttemptAsync(UserEventMessage userEvent, CancellationToken ct)
        {
            Guard.NotNull(userEvent, nameof(userEvent));

            var counterMap = CounterMap.ForNotification(ProcessStatus.Attempt);
            var counterKey = CounterKey.ForUserEvent(userEvent);

            return StoreCountersAsync(counterKey, counterMap);
        }

        public Task TrackFailedAsync(UserEventMessage userEvent, CancellationToken ct)
        {
            Guard.NotNull(userEvent, nameof(userEvent));

            var counterMap = CounterMap.ForNotification(ProcessStatus.Failed);
            var counterKey = CounterKey.ForUserEvent(userEvent);

            return StoreCountersAsync(counterKey, counterMap);
        }

        public Task InsertAsync(UserNotification notification, CancellationToken ct)
        {
            Guard.NotNull(notification, nameof(notification));

            var counterMap = CounterMap.ForNotification(ProcessStatus.Handled, 1);
            var counterKey = CounterKey.ForNotification(notification);

            return Task.WhenAll(
                StoreCountersAsync(counterKey, counterMap),
                StoreInternalAsync(notification, ct));
        }

        public Task CollectAndUpdateAsync(IUserNotification notification, string configuration, string channel, ProcessStatus status, string? detail)
        {
            Guard.NotNull(notification, nameof(notification));
            Guard.NotNullOrEmpty(channel, nameof(channel));

            var counterMap = CounterMap.ForChannel(channel, status);
            var counterKey = CounterKey.ForNotification(notification);

            return Task.WhenAll(
                StoreCountersAsync(counterKey, counterMap),
                StoreInternalAsync(notification.Id, channel, configuration, status, detail));
        }

        public Task CollectAsync(IUserNotification notification, string channel, ProcessStatus status)
        {
            Guard.NotNull(notification, nameof(notification));
            Guard.NotNullOrEmpty(channel, nameof(channel));

            var counterMap = CounterMap.ForChannel(channel, status);
            var counterKey = CounterKey.ForNotification(notification);

            return StoreCountersAsync(counterKey, counterMap);
        }

        private async Task StoreInternalAsync(Guid id, string channel, string configuration, ProcessStatus status, string? detail)
        {
            var info = CreateInfo(status, detail);

            await collector.AddAsync(id, channel, configuration, info);
        }

        private Task StoreCountersAsync(CounterKey key, CounterMap counterValues)
        {
            return counters.CollectAsync(key, counterValues);
        }

        private Task StoreInternalAsync(UserNotification notification, CancellationToken ct)
        {
            return repository.InsertAsync(notification, ct);
        }

        private ChannelSendInfo CreateInfo(ProcessStatus status, string? detail)
        {
            var now = clock.GetCurrentInstant();

            return new ChannelSendInfo { LastUpdate = now, Detail = detail, Status = status };
        }

        private HandledInfo CreateHandle(string? channel)
        {
            var now = clock.GetCurrentInstant();

            return new HandledInfo { Timestamp = now, Channel = channel };
        }
    }
}
