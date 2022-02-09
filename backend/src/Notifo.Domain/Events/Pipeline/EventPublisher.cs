// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Infrastructure;
using IEventProducer = Notifo.Infrastructure.Messaging.IMessageProducer<Notifo.Domain.Events.EventMessage>;

namespace Notifo.Domain.Events.Pipeline
{
    public sealed class EventPublisher : IEventPublisher
    {
        private static readonly Duration MaxAge = Duration.FromHours(1);
        private readonly IEventProducer producer;
        private readonly ILogStore logStore;
        private readonly ILogger<EventPublisher> log;
        private readonly IClock clock;

        public EventPublisher(IEventProducer producer, ILogStore logStore,
            ILogger<EventPublisher> log, IClock clock)
        {
            this.producer = producer;
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
                        await logStore.LogAsync(message.AppId, Texts.Events_TooOld);
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

                await ProduceAsync(message);

                log.LogInformation("Received event for app {appId} with ID {id} to topic {topic}.",
                    message.AppId,
                    message.Id,
                    message.Topic);
            }
        }

        private async Task ProduceAsync(EventMessage message)
        {
            await producer.ProduceAsync(message.AppId, message);
        }
    }
}
