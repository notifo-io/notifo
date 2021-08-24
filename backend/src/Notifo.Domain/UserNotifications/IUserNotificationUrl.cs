// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Notifo.Domain.UserNotifications
{
    public interface IUserNotificationUrl
    {
        string TrackConfirmed(Guid notificationId, string language);

        string TrackDelivered(Guid notificationId, string language);

        string TrackSeen(Guid notificationId, string language);
    }
}
