// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Infrastructure;
using Squidex.Log;
using IEventProducer = Notifo.Infrastructure.Messaging.IAbstractProducer<Notifo.Domain.Events.EventMessage>;

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
            CancellationToken ct)
        {
            Guard.NotNull(message, nameof(message));

            using (Telemetry.Activities.StartActivity("PublishEvent"))
            {
                message.Validate();

                message.Enqueued = clock.GetCurrentInstant();

                if (message.Created == default)
                {
                    message.Created = message.Enqueued;
                    message.Enqueued = message.Created;
                }
                else
                {
                    var age = message.Enqueued - message.Created;

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

                await producer.ProduceAsync(message.AppId, message);

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
