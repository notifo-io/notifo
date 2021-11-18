// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Confluent.Kafka;
using Notifo.Infrastructure.Json;
using Squidex.Log;

namespace Notifo.Infrastructure.Messaging.Implementation.Kafka
{
    public sealed class KafkaMessaging<T> : IMessaging<T>
    {
        private readonly KafkaMessagingProvider provider;
        private readonly ISemanticLog log;
        private readonly string topicName;
        private readonly KafkaJsonSerializer<Envelope<T>> serializer;
        private IProducer<string, Envelope<T>>? producer;
        private KafkaConsumer<T>? consumer;

        public int Order => 5000;

        public string Name => $"KafkaMessaging({topicName})";

        public KafkaMessaging(KafkaMessagingProvider provider, string topicName,
            IJsonSerializer serializer, ISemanticLog log)
        {
            this.provider = provider;
            this.serializer = new KafkaJsonSerializer<Envelope<T>>(serializer);
            this.topicName = topicName;
            this.log = log;
        }

        public Task InitializeAsync(
            CancellationToken ct)
        {
            producer =
                new DependentProducerBuilder<string, Envelope<T>>(provider.Handle)
                    .SetValueSerializer(serializer)
                    .Build();

            return Task.CompletedTask;
        }

        public Task ReleaseAsync(
            CancellationToken ct)
        {
            if (producer != null)
            {
                producer.Flush(ct);
                producer.Dispose();
            }

            consumer?.Dispose();

            return Task.CompletedTask;
        }

        public async Task ProduceAsync(string key, Envelope<T> envelope,
            CancellationToken ct = default)
        {
            if (producer == null)
            {
                throw new InvalidOperationException("Producer not initialized yet.");
            }

            var message = new Message<string, Envelope<T>> { Value = envelope, Key = key };

            try
            {
                producer.Produce(topicName, message);
            }
            catch (ProduceException<string, T> ex) when (ex.Error.Code == ErrorCode.Local_QueueFull)
            {
                while (true)
                {
                    try
                    {
                        producer.Poll(Timeout.InfiniteTimeSpan);

                        await producer.ProduceAsync(topicName, message, ct);

                        return;
                    }
                    catch (ProduceException<string, T> ex2) when (ex2.Error.Code == ErrorCode.Local_QueueFull)
                    {
                        await Task.Delay(100, ct);
                    }
                }
            }
        }

        public Task SubscribeAsync(MessageCallback<T> onMessage,
            CancellationToken ct = default)
        {
            consumer = new KafkaConsumer<T>(provider, topicName, onMessage, serializer, log);
            consumer.Subscribe();

            return Task.CompletedTask;
        }
    }
}
