// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using NodaTime;
using Notifo.Infrastructure;
using IEventProducer = Notifo.Infrastructure.Messaging.IAbstractProducer<Notifo.Domain.Events.EventMessage>;

namespace Notifo.Domain.Events.Pipeline
{
    public sealed class EventPublisher : IEventPublisher
    {
        private static readonly Duration MaxAge = Duration.FromHours(1);
        private readonly IEventProducer producer;
        private readonly IClock clock;

        public EventPublisher(IEventProducer producer, IClock clock)
        {
            this.producer = producer;

            this.clock = clock;
        }

        public async Task PublishAsync(EventMessage message)
        {
            Guard.NotNull(message, nameof(message));

            message.Validate();

            var now = clock.GetCurrentInstant();

            if (message.Created == default)
            {
                message.Created = clock.GetCurrentInstant();
            }
            else
            {
                var age = now - message.Created;

                if (age > MaxAge)
                {
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(message.Id))
            {
                message.Id = Guid.NewGuid().ToString();
            }

            await producer.ProduceAsync(message.AppId, message);
        }
    }
}
