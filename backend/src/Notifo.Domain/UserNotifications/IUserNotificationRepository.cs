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
        Task<bool> IsConfirmedOrHandled(Guid id, string channel, string configuration);

        Task<IResultList<UserNotification>> QueryAsync(string appId, string userId, UserNotificationQuery query, CancellationToken ct);

        Task<UserNotification?> FindAsync(Guid id);

        Task<UserNotification?> TrackConfirmedAsync(Guid id, HandledInfo handle);

        Task DeleteAsync(Guid id, CancellationToken ct);

        Task TrackSeenAsync(IEnumerable<Guid> ids, HandledInfo handle);

        Task InsertAsync(UserNotification notification, CancellationToken ct);

        Task UpdateAsync(IEnumerable<(Guid Id, string Channel, string Configuraton, ChannelSendInfo Info)> updates, CancellationToken ct);
    }
}
