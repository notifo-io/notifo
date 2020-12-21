// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Notifo.Infrastructure.Initialization;
using Squidex.Log;

namespace Notifo.Infrastructure.Messaging.Kafka
{
    public sealed class KafkaProvider : IMessagingProvider, IDisposable
    {
        private readonly IProducer<Null, Null> producer;

        public Handle Handle => producer.Handle;

        public KafkaProvider(IOptions<KafkaOptions> options, ISemanticLog log)
        {
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

        public IInitializable GetConsumer<TConsumer, T>(IServiceProvider serviceProvider, string channelName) where TConsumer : IAbstractConsumer<T>
        {
            return ActivatorUtilities.CreateInstance<KafkaConsumer<TConsumer, T>>(serviceProvider, channelName);
        }

        public IAbstractProducer<T> GetProducer<T>(IServiceProvider serviceProvider, string channelName)
        {
            return ActivatorUtilities.CreateInstance<KafkaProducer<T>>(serviceProvider, channelName);
        }
    }
}
