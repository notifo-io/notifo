﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Notifo.Domain.Log;
using Notifo.Infrastructure;
using Squidex.Messaging;

namespace Notifo.Domain.Events.Pipeline;

public sealed class EventPublisher(
    IMessageBus messageBus,
    ILogStore logStore,
    ILogger<EventPublisher> log,
    IClock clock)
    : IEventPublisher
{
    private static readonly Duration MaxAge = Duration.FromHours(1);

    public async Task PublishAsync(EventMessage message,
        CancellationToken ct = default)
    {
        Guard.NotNull(message);

        using (var activity = Telemetry.Activities.StartActivity("PublishEvent"))
        {
            message.Validate();

            var now = clock.GetCurrentInstant();

            if (message.Created == default)
            {
                message.Created = now;
            }
            else
            {
                var age = now - message.Created;

                if (age > MaxAge)
                {
                    await logStore.LogAsync(message.AppId, LogMessage.Event_TooOld("System"));
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(message.Id))
            {
                message.Id = Guid.NewGuid().ToString();
            }

            if (activity != null)
            {
                message.EventActivity = activity.Context;
            }

            await messageBus.PublishAsync(message, message.AppId, ct);

            log.LogInformation("Received event for app {appId} with ID {id} to topic {topic}.",
                message.AppId,
                message.Id,
                message.Topic);
        }
    }
}
