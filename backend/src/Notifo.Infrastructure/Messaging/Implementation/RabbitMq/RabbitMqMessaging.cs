// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.Json;

namespace Notifo.Infrastructure.Messaging.Implementation.RabbitMq
{
    public sealed class RabbitMqMessaging<T> : IMessaging<T>
    {
        private readonly RabbitMqMessagingProvider provider;
        private readonly RabbitMqProducer producer;
        private readonly string queueName;
        private readonly IJsonSerializer serializer;
        private RabbitMqConsumer<T>? consumer;

        public int Order => 5000;

        public string Name => $"RabbitMqMessaging({queueName})";

        public RabbitMqMessaging(
            RabbitMqMessagingProvider provider,
            RabbitMqProducer producer,
            string queueName, IJsonSerializer serializer)
        {
            this.producer = producer;
            this.provider = provider;
            this.queueName = queueName;
            this.serializer = serializer;
        }

        public Task InitializeAsync(
            CancellationToken ct)
        {
            producer.CreateQueue(queueName);

            return Task.CompletedTask;
        }

        public Task ProduceAsync(string key, Envelope<T> envelope,
            CancellationToken ct = default)
        {
            var bytes = serializer.SerializeToBytes(envelope);

            producer.Publish(queueName, bytes);

            return Task.CompletedTask;
        }

        public Task SubscribeAsync(MessageCallback<T> onMessage,
            CancellationToken ct = default)
        {
            consumer = new RabbitMqConsumer<T>(provider, queueName, onMessage, serializer);
            consumer.Subscribe();

            return Task.CompletedTask;
        }
    }
}
