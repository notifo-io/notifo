// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

        Task SendAsync(UserNotification notification, NotificationSetting setting, User user, App app, bool isUpdate, CancellationToken ct = default);

        bool CanSend(UserNotification notification, NotificationSetting setting, User user, App app)
        {
            return true;
        }
    }
}
