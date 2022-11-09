// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Channels;
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

        public UserNotificationStore(IUserNotificationRepository repository, ICounterService counters,
            IClock clock)
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

        public Task<IResultList<UserNotification>> QueryAsync(string appId, string userId, UserNotificationQuery query,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId);
            Guard.NotNullOrEmpty(userId);
            Guard.NotNull(query);

            return repository.QueryAsync(appId, userId, query, ct);
        }

        public Task<IReadOnlyDictionary<string, Instant>> QueryLastNotificationsAsync(string appId, IEnumerable<string> userIds,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId);
            Guard.NotNull(userIds);

            return repository.QueryLastNotificationsAsync(appId, userIds, ct);
        }

        public Task<UserNotification?> FindAsync(Guid id,
            CancellationToken ct = default)
        {
            return repository.FindAsync(id, ct);
        }

        public Task DeleteAsync(Guid id,
            CancellationToken ct = default)
        {
            return repository.DeleteAsync(id, ct);
        }

        public Task<bool> IsHandledAsync(ChannelJob job, ICommunicationChannel channel,
            CancellationToken ct = default)
        {
            Guard.NotNull(job);

            var notificationId = job.Tracking.UserNotificationId;

            if (job.IsUpdate || job.Delay <= Duration.Zero || notificationId == default)
            {
                return Task.FromResult(false);
            }

            switch (job.Condition)
            {
                case ChannelCondition.IfNotConfirmed:
                    return repository.IsHandledOrConfirmedAsync(notificationId, channel.Name, job.ConfigurationId, ct);
                case ChannelCondition.IfNotSeen:
                    return repository.IsHandledOrSeenAsync(notificationId, channel.Name, job.ConfigurationId, ct);
                default:
                    return repository.IsHandledAsync(notificationId, channel.Name, job.ConfigurationId, ct);
            }
        }

        public Task<bool> IsHandledOrSeenAsync(Guid id, string channel, Guid configurationId,
            CancellationToken ct = default)
        {
            return repository.IsHandledOrSeenAsync(id, channel, configurationId, ct);
        }

        public Task<UserNotification?> TrackConfirmedAsync(TrackingToken token,
            CancellationToken ct = default)
        {
            Guard.NotDefault(token);

            return repository.TrackConfirmedAsync(token, clock.GetCurrentInstant(), ct);
        }

        public Task TrackDeliveredAsync(IEnumerable<TrackingToken> tokens,
            CancellationToken ct = default)
        {
            Guard.NotNull(tokens);

            return repository.TrackDeliveredAsync(tokens, clock.GetCurrentInstant(), ct);
        }

        public Task TrackSeenAsync(IEnumerable<TrackingToken> tokens,
            CancellationToken ct = default)
        {
            Guard.NotNull(tokens);

            return repository.TrackSeenAsync(tokens, clock.GetCurrentInstant(), ct);
        }

        public Task TrackAttemptAsync(UserEventMessage userEvent,
            CancellationToken ct = default)
        {
            Guard.NotNull(userEvent);

            var counterMap = CounterMap.ForNotification(ProcessStatus.Attempt);
            var counterKey = TrackingKey.ForUserEvent(userEvent);

            return StoreCountersAsync(counterKey, counterMap, ct);
        }

        public Task TrackFailedAsync(UserEventMessage userEvent,
            CancellationToken ct = default)
        {
            Guard.NotNull(userEvent);

            var counterMap = CounterMap.ForNotification(ProcessStatus.Failed);
            var counterKey = TrackingKey.ForUserEvent(userEvent);

            return StoreCountersAsync(counterKey, counterMap, ct);
        }

        public Task InsertAsync(UserNotification notification,
            CancellationToken ct = default)
        {
            Guard.NotNull(notification);

            var counterMap = CounterMap.ForNotification(ProcessStatus.Handled);
            var counterKey = TrackingKey.ForNotification(notification);

            return Task.WhenAll(
                StoreCountersAsync(counterKey, counterMap, ct),
                StoreInternalAsync(notification, ct));
        }

        public Task TrackAsync(TrackingKey identifier, ProcessStatus status, string? detail = null,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(identifier.Channel);

            var counterMap = CounterMap.ForChannel(identifier.Channel!, status);
            var counterKey = identifier;

            if (identifier.ConfigurationId == default)
            {
                return StoreCountersAsync(counterKey, counterMap, ct);
            }

            return Task.WhenAll(
                StoreCountersAsync(counterKey, counterMap, ct),
                StoreInternalAsync(identifier.UserNotificationId, identifier.Channel!, identifier.ConfigurationId, status, detail));
        }

        private Task StoreCountersAsync(TrackingKey key, CounterMap counterValues,
            CancellationToken ct)
        {
            return counters.CollectAsync(key, counterValues, ct);
        }

        private Task StoreInternalAsync(UserNotification notification,
            CancellationToken ct)
        {
            return repository.InsertAsync(notification, ct);
        }

        private async Task StoreInternalAsync(Guid id, string channel, Guid configurationId, ProcessStatus status, string? detail)
        {
            var info = CreateInfo(status, detail);

            await collector.AddAsync(id, channel, configurationId, info);
        }

        private ChannelSendInfo CreateInfo(ProcessStatus status, string? detail)
        {
            var now = clock.GetCurrentInstant();

            return new ChannelSendInfo { LastUpdate = now, Detail = detail, Status = status };
        }
    }
}
