// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;
using Squidex.Messaging;

namespace Notifo.Domain.UserEvents.Pipeline;

public sealed class UserEventConsumer(IUserNotificationService userNotificationService) : IMessageHandler<UserEventMessage>
{
    public async Task HandleAsync(UserEventMessage message,
        CancellationToken ct)
    {
        var activityLinks = message.Links();
        var activityContext = Activity.Current?.Context ?? default;

        using (Telemetry.Activities.StartActivity("ConsumeUserEvent", ActivityKind.Internal, activityContext, links: activityLinks))
        {
            await userNotificationService.DistributeAsync(message);
        }
    }
}
