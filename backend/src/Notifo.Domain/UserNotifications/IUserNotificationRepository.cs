// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure;

namespace Notifo.Domain.UserNotifications;

public interface IUserNotificationRepository
{
    Task<bool> IsHandledOrConfirmedAsync(Guid id, string channel, Guid configurationId,
        CancellationToken ct = default);

    Task<bool> IsHandledOrSeenAsync(Guid id, string channel, Guid configurationId,
        CancellationToken ct = default);

    Task<bool> IsHandledAsync(Guid id, string channel, Guid configurationId,
        CancellationToken ct = default);

    Task<IResultList<UserNotification>> QueryAsync(string appId, string userId, UserNotificationQuery query,
        CancellationToken ct = default);

    Task<IResultList<UserNotification>> QueryAsync(string appId, UserNotificationQuery query,
        CancellationToken ct = default);

    Task<IReadOnlyDictionary<string, Instant>> QueryLastNotificationsAsync(string appId, IEnumerable<string> userIds,
        CancellationToken ct = default);

    Task<UserNotification?> FindAsync(Guid id,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id,
        CancellationToken ct = default);

    Task InsertAsync(UserNotification notification,
        CancellationToken ct = default);

    Task<IReadOnlyList<(UserNotification, bool Updated)>> TrackConfirmedAsync(TrackingToken[] tokens, Instant now,
        CancellationToken ct = default);

    Task<IReadOnlyList<(UserNotification, bool Updated)>> TrackSeenAsync(TrackingToken[] tokens, Instant now,
        CancellationToken ct = default);

    Task<IReadOnlyList<(UserNotification, bool Updated)>> TrackDeliveredAsync(TrackingToken[] tokens, Instant now,
        CancellationToken ct = default);

    Task BatchWriteAsync((TrackingToken Token, DeliveryResult Result)[] updates, Instant now,
        CancellationToken ct = default);
}
