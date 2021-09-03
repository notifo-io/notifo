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
using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels
{
    public interface ICommunicationChannel
    {
        string Name { get; }

        bool IsSystem => false;

        Task SendAsync(UserNotification notification, NotificationSetting settings, string configuration, SendOptions options,
            CancellationToken ct);

        IEnumerable<string> GetConfigurations(UserNotification notification, NotificationSetting settings, SendOptions options);

        Task HandleDeliveredAsync(Guid id, TrackingDetails details)
        {
            return Task.CompletedTask;
        }

        Task HandleSeenAsync(Guid id, TrackingDetails details)
        {
            return Task.CompletedTask;
        }
    }
}
