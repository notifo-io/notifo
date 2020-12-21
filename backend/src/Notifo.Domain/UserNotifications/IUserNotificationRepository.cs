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

namespace Notifo.Domain.UserNotifications
{
    public interface IUserNotificationRepository
    {
        Task<bool> IsConfirmedOrHandled(Guid id, string channel);

        Task<List<UserNotification>> QueryAsync(string appId, string userId, int count, Instant after, CancellationToken ct);

        Task<UserNotification?> FindAsync(Guid id);

        Task<UserNotification?> TrackConfirmedAsync(Guid id, HandledInfo handle);

        Task TrackSeenAsync(IEnumerable<Guid> ids, HandledInfo handle);

        Task InsertAsync(UserNotification notification, CancellationToken ct);

        Task UpdateAsync(IEnumerable<(Guid Id, string Channel, ChannelSendInfo Info)> updates, CancellationToken ct);
    }
}
