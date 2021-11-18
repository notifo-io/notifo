// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.Web
{
    public sealed class NoopStreamClient : IStreamClient
    {
        public Task SendAsync(UserNotification userNotification)
        {
            return Task.CompletedTask;
        }
    }
}
