// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Confluent.Kafka;
using Squidex.Hosting.Configuration;

namespace Notifo.Infrastructure.Messaging.Kafka
{
    public sealed class KafkaOptions : ConsumerConfig, IValidatableOptions
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

        public IEnumerable<ConfigurationError> Validate()
        {
            if (string.IsNullOrWhiteSpace(BootstrapServers))
            {
                yield return new ConfigurationError("Value is required.", nameof(BootstrapServers));
            }
        }
    }
}
