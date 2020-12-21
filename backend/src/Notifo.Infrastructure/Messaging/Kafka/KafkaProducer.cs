// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Notifo.Infrastructure.Initialization;
using Notifo.Infrastructure.Json;

namespace Notifo.Infrastructure.Messaging.Kafka
{
    public sealed class KafkaProducer<T> : IAbstractProducer<T>, IInitializable, IDisposable
    {
        private readonly KafkaProvider provider;
        private readonly string topicName;
        private readonly IJsonSerializer serializer;
        private IProducer<string, T>? producer;

        public KafkaProducer(string topicName, KafkaProvider provider, IJsonSerializer serializer)
        {
            this.provider = provider;
            this.serializer = serializer;
            this.topicName = topicName;
        }

        public Task InitializeAsync(CancellationToken cancellationToken)
        {
            producer =
                new DependentProducerBuilder<string, T>(provider.Handle)
                    .SetValueSerializer(new KafkaJsonSerializer<T>(serializer))
                    .Build();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (producer != null)
            {
                producer.Flush();
                producer.Dispose();
            }
        }

        public async Task ProduceAsync(string key, T value)
        {
            if (producer == null)
            {
                throw new InvalidOperationException("Producer not initialized yet.");
            }

            var message = new Message<string, T> { Value = value, Key = key };

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

                        await producer.ProduceAsync(topicName, message);

                        return;
                    }
                    catch (ProduceException<string, T> ex2) when (ex2.Error.Code == ErrorCode.Local_QueueFull)
                    {
                        await Task.Delay(100);
                    }
                }
            }
        }
    }
}
