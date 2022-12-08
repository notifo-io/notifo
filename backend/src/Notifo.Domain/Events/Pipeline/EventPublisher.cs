// ==========================================================================
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

public sealed class EventPublisher : IEventPublisher
{
    private static readonly Duration MaxAge = Duration.FromHours(1);
    private readonly IMessageBus messageBus;
    private readonly ILogStore logStore;
    private readonly ILogger<EventPublisher> log;
    private readonly IClock clock;

    public EventPublisher(IMessageBus messageBus, ILogStore logStore,
        ILogger<EventPublisher> log, IClock clock)
    {
        this.messageBus = messageBus;
        this.logStore = logStore;
        this.log = log;
        this.clock = clock;
    }

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
                    await logStore.LogAsync(message.AppId, LogMessage.Events_TooOld("System"));
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
