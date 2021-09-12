// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using Confluent.Kafka;
using Squidex.Log;

namespace Notifo.Infrastructure.Messaging.Implementation.Kafka
{
    public sealed class KafkaConsumer<T>
    {
        private readonly KafkaMessagingProvider provider;
        private readonly string topicName;
        private readonly MessageCallback<T> onMessage;
        private readonly KafkaJsonSerializer<Envelope<T>> serializer;
        private readonly ISemanticLog log;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Thread consumerThread;

        public KafkaConsumer(KafkaMessagingProvider provider, string topicName, MessageCallback<T> onMessage,
            KafkaJsonSerializer<Envelope<T>> serializer, ISemanticLog log)
        {
            this.provider = provider;
            this.topicName = topicName;
            this.onMessage = onMessage;
            this.serializer = serializer;

            this.log = log;
        }

        public void Subscribe()
        {
#pragma warning disable IDE0017 // Simplify object initialization
            consumerThread = new Thread(Consume);
            consumerThread.Name = "MessagingConsumer";
            consumerThread.IsBackground = true;
            consumerThread.Start();
#pragma warning restore IDE0017 // Simplify object initialization
        }

        private void Consume(object? parameter)
        {
            var config = provider.Options.Configure(new ConsumerConfig
            {
                AutoOffsetReset = AutoOffsetReset.Earliest
            });

            var kafkaConsumer =
                new ConsumerBuilder<string, Envelope<T>>(config)
                    .SetLogHandler(KafkaLogFactory<string, Envelope<T>>.ConsumerLog(log))
                    .SetErrorHandler(KafkaLogFactory<string, Envelope<T>>.ConsumerError(log))
                    .SetStatisticsHandler(KafkaLogFactory<string, Envelope<T>>.ConsumerStats(log))
                    .SetValueDeserializer(serializer)
                    .Build();

            using (kafkaConsumer)
            {
                kafkaConsumer.Subscribe(topicName);

                try
                {
                    while (!cancellationTokenSource.IsCancellationRequested)
                    {
                        var result = kafkaConsumer.Consume(cancellationTokenSource.Token);

                        onMessage(result.Message.Value, cancellationTokenSource.Token).Wait(cancellationTokenSource.Token);
                    }
                }
                finally
                {
                    kafkaConsumer.Close();
                }
            }
        }

        public void Dispose()
        {
            try
            {
                cancellationTokenSource.Cancel();

                consumerThread.Join(1500);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "Shutdown")
                    .WriteProperty("system", "Kafka")
                    .WriteProperty("status", "Failed"));
            }
        }
    }
}
