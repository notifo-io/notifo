// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserEvents;
using Notifo.Infrastructure;

namespace Notifo.Domain.UserNotifications
{
    public interface IUserNotificationStore
    {
        Task<bool> IsConfirmedOrHandledAsync(Guid id, string channel, string configuration,
            CancellationToken ct = default);

        Task<IResultList<UserNotification>> QueryAsync(string appId, string userId, UserNotificationQuery query,
            CancellationToken ct = default);

        Task<UserNotification?> TrackConfirmedAsync(Guid id, TrackingDetails details = default,
            CancellationToken ct = default);

        Task<UserNotification?> FindAsync(Guid id,
            CancellationToken ct = default);

        Task DeleteAsync(Guid id,
            CancellationToken ct = default);

        Task TrackDeliveredAsync(IEnumerable<Guid> ids, TrackingDetails details = default,
            CancellationToken ct = default);

        Task TrackSeenAsync(IEnumerable<Guid> ids, TrackingDetails details = default,
            CancellationToken ct = default);

        Task TrackAttemptAsync(UserEventMessage userEvent,
            CancellationToken ct = default);

        Task TrackFailedAsync(UserEventMessage userEvent,
            CancellationToken ct = default);

        Task InsertAsync(UserNotification notification,
            CancellationToken ct = default);

        Task CollectAndUpdateAsync(IUserNotification notification, string channel, string configuration, ProcessStatus status, string? detail = null,
            CancellationToken ct = default);

        Task CollectAsync(IUserNotification notification, string channel, ProcessStatus status,
            CancellationToken ct = default);
    }
}
