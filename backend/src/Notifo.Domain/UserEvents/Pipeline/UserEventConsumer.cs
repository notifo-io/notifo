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

public sealed class UserEventConsumer : IMessageHandler<UserEventMessage>
{
    private readonly IUserNotificationService userNotificationService;

    public UserEventConsumer(IUserNotificationService userNotificationService)
    {
        this.userNotificationService = userNotificationService;
    }

    public async Task HandleAsync(UserEventMessage message,
        CancellationToken ct)
    {
        var links = message.Links();

        var parentContext = Activity.Current?.Context ?? default;

        using (Telemetry.Activities.StartActivity("ConsumeUserEvent", ActivityKind.Internal, parentContext, links: links))
        {
            await userNotificationService.DistributeAsync(message);
        }
    }
}
