// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Esprima.Ast;
using NodaTime;
using Notifo.Domain.Channels;
using Notifo.Domain.Counters;
using Notifo.Domain.Integrations;
using Notifo.Domain.UserEvents;
using Notifo.Domain.UserNotifications.Internal;
using Notifo.Infrastructure;

namespace Notifo.Domain.UserNotifications;

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

        collector = new StatisticsCollector(repository, clock, 5000);
    }

    public void Dispose()
    {
        collector.StopAsync();
    }

    public async Task<IResultList<UserNotification>> QueryForDeviceAsync(string appId, string userId, DeviceNotificationsQuery query,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(appId);
        Guard.NotNullOrEmpty(userId);
        Guard.NotNull(query);

        if (string.IsNullOrWhiteSpace(query.DeviceIdentifier))
        {
            return ResultList.Empty<UserNotification>();
        }

        var notifications = await repository.QueryAsync(appId, userId, query.ToBaseQuery(), ct);

        static bool IsMatching(ChannelSendInfo status, DeviceNotificationsQuery query)
        {
            if (!status.Configuration.ContainsValue(query.DeviceIdentifier!))
            {
                return false;
            }

            if (status.Status < DeliveryStatus.Sent)
            {
                return false;
            }

            switch (query.Scope)
            {
                case DeviceNotificationsQueryScope.Seen:
                    return status.FirstSeen != null;
                case DeviceNotificationsQueryScope.Unseen:
                    return status.FirstSeen == null;
                default:
                    return true;
            }
        }

        var filteredNotifications = notifications.Where(notification =>
        {
            if (notification.Silent)
            {
                return false;
            }

            if (!notification.Channels.TryGetValue(Providers.MobilePush, out var channel))
            {
                return false;
            }

            return channel.Status.Values.Any(x => IsMatching(x, query));
        });

        return ResultList.Create(filteredNotifications);
    }

    public Task<IResultList<UserNotification>> QueryAsync(string appId, string userId, UserNotificationQuery query,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(appId);
        Guard.NotNullOrEmpty(userId);
        Guard.NotNull(query);

        return repository.QueryAsync(appId, userId, query, ct);
    }

    public Task<IResultList<UserNotification>> QueryAsync(string appId, UserNotificationQuery query,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(appId);
        Guard.NotNull(query);

        return repository.QueryAsync(appId, query, ct);
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

        var notificationId = job.Notification.Id;

        if (job.IsUpdate || job.SendDelay <= Duration.Zero)
        {
            return Task.FromResult(false);
        }

        switch (job.SendCondition)
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

    public Task<IReadOnlyList<(UserNotification, bool Updated)>> TrackConfirmedAsync(TrackingToken[] tokens,
        CancellationToken ct = default)
    {
        Guard.NotNull(tokens);

        return repository.TrackConfirmedAsync(tokens, clock.GetCurrentInstant(), ct);
    }

    public Task<IReadOnlyList<(UserNotification, bool Updated)>> TrackSeenAsync(TrackingToken[] tokens,
        CancellationToken ct = default)
    {
        Guard.NotNull(tokens);

        return repository.TrackSeenAsync(tokens, clock.GetCurrentInstant(), ct);
    }

    public Task<IReadOnlyList<(UserNotification, bool Updated)>> TrackDeliveredAsync(TrackingToken[] tokens,
        CancellationToken ct = default)
    {
        Guard.NotNull(tokens);

        return repository.TrackDeliveredAsync(tokens, clock.GetCurrentInstant(), ct);
    }

    public Task TrackAsync(UserEventMessage userEvent, DeliveryResult result,
        CancellationToken ct = default)
    {
        Guard.NotNull(userEvent);

        var counterMap = CounterMap.ForNotification(result.Status);
        var counterKey = TrackingKey.ForUserEvent(userEvent);

        return StoreCountersAsync(counterKey, counterMap, ct);
    }

    public Task InsertAsync(UserNotification notification,
        CancellationToken ct = default)
    {
        Guard.NotNull(notification);

        var counterMap = CounterMap.ForNotification(DeliveryStatus.Attempt);
        var counterKey = TrackingKey.ForNotification(notification);

        return Task.WhenAll(
            StoreCountersAsync(counterKey, counterMap, ct),
            StoreInternalAsync(notification, ct));
    }

    public Task TrackAsync(TrackingKey identifier, DeliveryResult result,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(identifier.Channel);

        var counterMap = CounterMap.ForChannel(identifier.Channel!, result.Status);
        var counterKey = identifier;

        if (identifier.ConfigurationId == default)
        {
            return StoreCountersAsync(counterKey, counterMap, ct);
        }

        return Task.WhenAll(
            StoreCountersAsync(counterKey, counterMap, ct),
            StoreInternalAsync(identifier.ToToken(), result));
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

    private async Task StoreInternalAsync(TrackingToken token, DeliveryResult result)
    {
        await collector.AddAsync(token, result);
    }
}
