// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Notifo.Domain.UserNotifications
{
    public interface IUserNotification
    {
        public Guid Id { get; }

        public string EventId { get; }

        public string AppId { get; }

        public string UserId { get; }

        public string Topic { get; }
    }
}
