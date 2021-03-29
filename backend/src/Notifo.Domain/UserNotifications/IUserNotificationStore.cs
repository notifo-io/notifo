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
using Notifo.Domain.UserEvents;
using Notifo.Infrastructure;

namespace Notifo.Domain.UserNotifications
{
    public interface IUserNotificationStore
    {
        Task<bool> IsConfirmed(Guid id, string channel);

        Task<IResultList<UserNotification>> QueryAsync(string appId, string userId, UserNotificationQuery query, CancellationToken ct = default);

        Task<UserNotification?> TrackConfirmedAsync(Guid id, string? channel = null);

        Task<UserNotification?> FindAsync(Guid id);

        Task DeleteAsync(Guid id, CancellationToken ct = default);

        Task TrackSeenAsync(IEnumerable<Guid> ids, string? channel = null);

        Task TrackAttemptAsync(UserEventMessage userEvent, CancellationToken ct = default);

        Task TrackFailedAsync(UserEventMessage userEvent, CancellationToken ct = default);

        Task InsertAsync(UserNotification notification, CancellationToken ct = default);

        Task CollectAndUpdateAsync(IUserNotification notification, string channel, ProcessStatus status, string? detail = null);

        Task CollectAsync(IUserNotification notification, string channel, ProcessStatus status);
    }
}
