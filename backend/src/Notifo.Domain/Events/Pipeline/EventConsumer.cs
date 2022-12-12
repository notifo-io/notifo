// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Notifo.Domain.UserEvents;
using Notifo.Infrastructure;
using Squidex.Messaging;

namespace Notifo.Domain.Events.Pipeline;

public sealed class EventConsumer : IMessageHandler<EventMessage>
{
    private readonly IUserEventPublisher userEventPublisher;

    public EventConsumer(IUserEventPublisher userEventPublisher)
    {
        this.userEventPublisher = userEventPublisher;
    }

    public async Task HandleAsync(EventMessage message,
        CancellationToken ct)
    {
        var activityLinks = message.Links();
        var activityContext = Activity.Current?.Context ?? default;

        using (Telemetry.Activities.StartActivity("ConsumeEvent", ActivityKind.Internal, activityContext, links: activityLinks))
        {
            await userEventPublisher.PublishAsync(message, ct);
        }
    }
}
