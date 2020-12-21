// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Notifo.Infrastructure.Initialization;
using Notifo.Infrastructure.Json;
using Squidex.Log;

namespace Notifo.Infrastructure.Messaging.Kafka
{
    public sealed class KafkaConsumer<TConsumer, T> : IInitializable, IDisposable where TConsumer : IAbstractConsumer<T>
    {
        private readonly KafkaOptions options;
        private readonly string topicName;
        private readonly TConsumer consumer;
        private readonly IJsonSerializer serializer;
        private readonly ISemanticLog log;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Thread consumerThread;

        public KafkaConsumer(string topicName, TConsumer consumer, IJsonSerializer serializer,
            IOptions<KafkaOptions> options, ISemanticLog log)
        {
            this.consumer = consumer;
            this.options = options.Value;
            this.serializer = serializer;
            this.topicName = topicName;

            this.log = log;
        }

        public Task InitializeAsync(CancellationToken cancellationToken)
        {
            var config = options.Configure(new ConsumerConfig
            {
                AutoOffsetReset = AutoOffsetReset.Earliest
            });

            consumerThread = new Thread(Consume) { IsBackground = true, Name = "EventConsumer" };
            consumerThread.Start(config);

            return Task.CompletedTask;
        }

        private void Consume(object? parameter)
        {
            var config = (ConsumerConfig)parameter!;

            var kafkaConsumer =
                new ConsumerBuilder<string, T>(config)
                    .SetLogHandler(KafkaLogFactory<string, T>.ConsumerLog(log))
                    .SetErrorHandler(KafkaLogFactory<string, T>.ConsumerError(log))
                    .SetStatisticsHandler(KafkaLogFactory<string, T>.ConsumerStats(log))
                    .SetValueDeserializer(new KafkaJsonSerializer<T>(serializer))
                    .Build();

            using (kafkaConsumer)
            {
                kafkaConsumer.Subscribe(topicName);

                try
                {
                    while (!cancellationTokenSource.IsCancellationRequested)
                    {
                        try
                        {
                            var result = kafkaConsumer.Consume(cancellationTokenSource.Token);

                            consumer.HandleAsync(result.Message.Value, cancellationTokenSource.Token).Wait();
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }
                        catch (JsonException ex)
                        {
                            log.LogError(ex, w => w
                                .WriteProperty("action", "ConsumeMessage")
                                .WriteProperty("system", "Kafka")
                                .WriteProperty("status", "Failed")
                                .WriteProperty("reason", "Failed to deserialize from JSON.")
                                .WriteProperty("topic", topicName));
                        }
                        catch (Exception ex)
                        {
                            log.LogError(ex, w => w
                                .WriteProperty("action", "ConsumeMessage")
                                .WriteProperty("system", "Kafka")
                                .WriteProperty("status", "Failed")
                                .WriteProperty("topic", topicName));
                        }
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
