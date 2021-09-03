// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using Notifo.Infrastructure.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Notifo.Infrastructure.Messaging.Implementation.RabbitMq
{
    public sealed class RabbitMqConsumer<T>
    {
        private readonly List<(IModel Channel, AsyncEventingBasicConsumer Consumer)> consumers = new List<(IModel Channel, AsyncEventingBasicConsumer Consumer)>();
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly RabbitMqMessagingProvider provider;
        private readonly string queueName;
        private readonly MessageCallback<T> onMessage;
        private readonly IJsonSerializer serializer;

        public RabbitMqConsumer(RabbitMqMessagingProvider provider, string queueName, MessageCallback<T> onMessage,
            IJsonSerializer serializer)
        {
            this.provider = provider;
            this.queueName = queueName;
            this.onMessage = onMessage;
            this.serializer = serializer;
        }

        public void Dispose()
        {
            cts.Cancel();

            foreach (var (channel, _) in consumers)
            {
                channel.Dispose();
            }
        }

        public void Subscribe()
        {
            using (var channel = provider.Connection.CreateModel())
            {
                channel.QueueDeclare(queueName, true, false, false, null);
            }

            for (var i = 0; i < provider.Options.MaxDegreeOfParallelism; i++)
            {
                CreateConsumer();
            }
        }

        private void CreateConsumer()
        {
            var channel = provider.Connection.CreateModel();

            var eventConsumer = new AsyncEventingBasicConsumer(channel);

            eventConsumer.Received += async (_, @event) =>
            {
                var message = serializer.Deserialize<Envelope<T>>(@event.Body.Span);

                await onMessage(message, cts.Token);

                channel.BasicAck(@event.DeliveryTag, true);
            };

            channel.BasicConsume(queueName, false, eventConsumer);

            consumers.Add((channel, eventConsumer));
        }
    }
}
