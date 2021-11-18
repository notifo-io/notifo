﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Infrastructure;
using Squidex.Log;
using IEventProducer = Notifo.Infrastructure.Messaging.IMessageProducer<Notifo.Domain.Events.EventMessage>;

namespace Notifo.Domain.Events.Pipeline
{
    public sealed class EventPublisher : IEventPublisher
    {
        private static readonly Duration MaxAge = Duration.FromHours(1);
        private readonly IEventProducer producer;
        private readonly ILogStore logStore;
        private readonly ISemanticLog log;
        private readonly IClock clock;

        public EventPublisher(IEventProducer producer, ILogStore logStore, ISemanticLog log, IClock clock)
        {
            this.producer = producer;
            this.logStore = logStore;
            this.log = log;
            this.clock = clock;
        }

        public async Task PublishAsync(EventMessage message,
            CancellationToken ct = default)
        {
            Guard.NotNull(message, nameof(message));

            using (Telemetry.Activities.StartActivity("PublishEvent"))
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
                        await logStore.LogAsync(message.AppId, Texts.Events_TooOld);
                        return;
                    }
                }

                if (string.IsNullOrWhiteSpace(message.Id))
                {
                    message.Id = Guid.NewGuid().ToString();
                }

#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods that take one
#pragma warning disable MA0040 // Flow the cancellation token
                await producer.ProduceAsync(message.AppId, message);
#pragma warning restore MA0040 // Flow the cancellation token
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods that take one

                log.LogInformation(message, (m, w) => w
                    .WriteProperty("action", "EventReceived")
                    .WriteProperty("appId", m.AppId)
                    .WriteProperty("eventId", m.Id)
                    .WriteProperty("eventTopic", m.Topic)
                    .WriteProperty("eventType", m.ToString()));
            }
        }
    }
}
