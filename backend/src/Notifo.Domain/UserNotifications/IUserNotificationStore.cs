// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Channels;
using Notifo.Domain.UserEvents;
using Notifo.Infrastructure;

namespace Notifo.Domain.UserNotifications;

public interface IUserNotificationStore
{
    Task<bool> IsHandledAsync(ChannelJob job, ICommunicationChannel channel,
        CancellationToken ct = default);

    Task<IResultList<UserNotification>> QueryAsync(string appId, string userId, UserNotificationQuery query,
        CancellationToken ct = default);

    Task<IResultList<UserNotification>> QueryAsync(string appId, UserNotificationQuery query,
        CancellationToken ct = default);

    Task<IResultList<UserNotification>> QueryForDeviceAsync(string appId, string userId, DeviceNotificationsQuery query,
        CancellationToken ct = default);

    Task<IReadOnlyDictionary<string, Instant>> QueryLastNotificationsAsync(string appId, IEnumerable<string> userIds,
        CancellationToken ct = default);

    Task<UserNotification?> FindAsync(Guid id,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id,
        CancellationToken ct = default);

    Task InsertAsync(UserNotification notification,
        CancellationToken ct = default);

    Task<IReadOnlyList<(UserNotification, bool Updated)>> TrackConfirmedAsync(TrackingToken[] tokens,
        CancellationToken ct = default);

    Task<IReadOnlyList<(UserNotification, bool Updated)>> TrackSeenAsync(TrackingToken[] tokens,
        CancellationToken ct = default);

    Task<IReadOnlyList<(UserNotification, bool Updated)>> TrackDeliveredAsync(TrackingToken[] tokens,
        CancellationToken ct = default);

    Task TrackAsync(UserEventMessage userEvent, ProcessStatus status,
        CancellationToken ct = default);

    Task TrackAsync(TrackingKey identifier, ProcessStatus status, string? detail = null,
        CancellationToken ct = default);
}
