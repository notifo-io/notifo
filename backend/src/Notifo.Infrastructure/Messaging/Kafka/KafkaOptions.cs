// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Confluent.Kafka;

namespace Notifo.Infrastructure.Messaging.Kafka
{
    public sealed class KafkaOptions : ConsumerConfig
    {
        public T Configure<T>(T config) where T : ClientConfig
        {
            if (config is ConsumerConfig consumerConfig)
            {
                consumerConfig.GroupId = GroupId;
            }

            foreach (var (key, value) in this)
            {
                config.Set(key, value);
            }

            return config;
        }
    }
}
