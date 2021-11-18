// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Log;

namespace Notifo.Infrastructure.Messaging.Implementation.Kafka
{
    public sealed class KafkaMessagingProvider : IMessagingProvider, IDisposable
    {
        private readonly IProducer<Null, Null> producer;

        public Handle Handle => producer.Handle;

        public KafkaOptions Options { get; }

        public KafkaMessagingProvider(IOptions<KafkaOptions> options, ISemanticLog log)
        {
            Options = options.Value;

            producer =
                new ProducerBuilder<Null, Null>(options.Value)
                    .SetLogHandler(KafkaLogFactory<Null, Null>.ProducerLog(log))
                    .SetErrorHandler(KafkaLogFactory<Null, Null>.ProducerError(log))
                    .SetStatisticsHandler(KafkaLogFactory<Null, Null>.ProducerStats(log))
                    .Build();
        }

        public void Dispose()
        {
            producer.Dispose();
        }

        public IMessaging<T> GetMessaging<T>(IServiceProvider serviceProvider, string channelName)
        {
            return ActivatorUtilities.CreateInstance<KafkaMessaging<T>>(serviceProvider, channelName);
        }
    }
}
