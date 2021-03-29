// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Apps;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;

namespace Notifo.Domain.Channels
{
    public interface ICommunicationChannel
    {
        string Name { get; }

        bool IsSystem => false;

        Task SendAsync(UserNotification notification, NotificationSetting setting, User user, App app, SendOptions options, CancellationToken ct = default);

        Task HandleSeenAsync(Guid id, SeenOptions options)
        {
            return Task.CompletedTask;
        }

        bool CanSend(UserNotification notification, NotificationSetting setting, User user, App app)
        {
            return true;
        }
    }
}
