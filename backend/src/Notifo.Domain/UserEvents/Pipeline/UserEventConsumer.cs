// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Messaging;

namespace Notifo.Domain.UserEvents.Pipeline
{
    public sealed class UserEventConsumer : IMessageHandler<UserEventMessage>
    {
        private readonly IUserNotificationService userNotificationService;

        public UserEventConsumer(IUserNotificationService userNotificationService)
        {
            this.userNotificationService = userNotificationService;
        }

        public async Task HandleAsync(UserEventMessage message,
            CancellationToken ct = default)
        {
            using (var trace = Telemetry.Activities.StartActivity("ConsumeUserEvent"))
            {
                await userNotificationService.DistributeAsync(message);
            }
        }
    }
}
