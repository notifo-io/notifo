// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Domain.UserEvents;
using Notifo.Domain.Users;

namespace Notifo.Domain.UserNotifications
{
    public interface IUserNotificationFactory
    {
        UserNotification? Create(App app, User user, UserEventMessage userEvent);
    }
}