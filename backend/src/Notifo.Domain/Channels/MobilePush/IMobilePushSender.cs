// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.MobilePush
{
    public interface IMobilePushSender
    {
        Task SendAsync(UserNotification userNotification, string token, bool wakeup, CancellationToken ct);
    }
}
