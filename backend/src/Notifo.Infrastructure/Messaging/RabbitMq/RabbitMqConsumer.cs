// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Squidex.Hosting;
using Squidex.Log;

namespace Notifo.Infrastructure.Messaging.RabbitMq
{
    public sealed class RabbitMqConsumer<T> : IInitializable
    {
        private readonly List<(IModel Channel, AsyncEventingBasicConsumer Consumer)> consumers = new List<(IModel Channel, AsyncEventingBasicConsumer Consumer)>();
        private readonly RabbitMqProvider connection;
        private readonly RabbitMqOptions options;
        private readonly string queueName;
        private readonly IJsonSerializer serializer;
        private readonly IAbstractConsumer<T> consumer;
        private readonly ISemanticLog log;

        public RabbitMqConsumer(RabbitMqProvider connection, RabbitMqOptions options, string queueName, IJsonSerializer serializer,
            IAbstractConsumer<T> consumer, ISemanticLog log)
        {
            this.connection = connection;
            this.consumer = consumer;
            this.options = options;
            this.queueName = queueName;
            this.serializer = serializer;

            this.log = log;
        }

        public void Dispose()
        {
            foreach (var (channel, _) in consumers)
            {
                channel.Dispose();
            }
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            using (var channel = connection.Connection.CreateModel())
            {
                channel.QueueDeclare(queueName, true, false, false, null);
            }

            for (var i = 0; i < options.MaxDegreeOfParallelism; i++)
            {
                CreateConsumer();
            }

            return Task.CompletedTask;
        }

        private void CreateConsumer()
        {
            var channel = connection.Connection.CreateModel();

            var eventConsumer = new AsyncEventingBasicConsumer(channel);

            eventConsumer.Received += async (_, @event) =>
            {
                try
                {
                    var message = serializer.Deserialize<T>(@event.Body.Span);

                    await consumer.HandleAsync(message, default);

                    channel.BasicAck(@event.DeliveryTag, true);
                }
                catch (JsonException ex)
                {
                    log.LogError(ex, w => w
                        .WriteProperty("action", "ConsumeMessage")
                        .WriteProperty("system", "RabbitMq")
                        .WriteProperty("status", "Failed")
                        .WriteProperty("reason", "Failed to deserialize from JSON.")
                        .WriteProperty("queue", queueName));

                    channel.BasicAck(@event.DeliveryTag, true);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, w => w
                        .WriteProperty("action", "ConsumeMessage")
                        .WriteProperty("system", "RabbitMq")
                        .WriteProperty("status", "Failed")
                        .WriteProperty("queue", queueName));
                }
            };

            channel.BasicConsume(queueName, false, eventConsumer);

            consumers.Add((channel, eventConsumer));
        }
    }
}
