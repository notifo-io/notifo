﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Messaging;

namespace Notifo.Domain.UserEvents.Pipeline
{
    public sealed class UserEventConsumer : IAbstractConsumer<UserEventMessage>
    {
        private readonly IUserNotificationService userNotificationService;

        public UserEventConsumer(IUserNotificationService userNotificationService)
        {
            this.userNotificationService = userNotificationService;
        }

        public Task HandleAsync(UserEventMessage message, CancellationToken ct)
        {
            return userNotificationService.DistributeAsync(message);
        }
    }
}
