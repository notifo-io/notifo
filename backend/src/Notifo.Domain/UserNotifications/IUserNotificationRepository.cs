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
using Notifo.Infrastructure;

namespace Notifo.Domain.UserNotifications
{
    public interface IUserNotificationRepository
    {
        Task<bool> IsConfirmedOrHandledAsync(Guid id, string channel, string configuration,
            CancellationToken ct = default);

        Task<IResultList<UserNotification>> QueryAsync(string appId, string userId, UserNotificationQuery query,
            CancellationToken ct = default);

        Task<UserNotification?> FindAsync(Guid id,
            CancellationToken ct = default);

        Task<UserNotification?> TrackConfirmedAsync(Guid id, HandledInfo handle,
            CancellationToken ct = default);

        Task DeleteAsync(Guid id,
            CancellationToken ct = default);

        Task TrackDeliveredAsync(IEnumerable<Guid> ids, HandledInfo handle,
            CancellationToken ct = default);

        Task TrackSeenAsync(IEnumerable<Guid> ids, HandledInfo handle,
            CancellationToken ct = default);

        Task InsertAsync(UserNotification notification,
            CancellationToken ct = default);

        Task BatchWriteAsync(IEnumerable<(Guid Id, string Channel, string Configuraton, ChannelSendInfo Info)> updates,
            CancellationToken ct = default);
    }
}
