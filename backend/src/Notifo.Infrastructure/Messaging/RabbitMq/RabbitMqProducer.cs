// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure.Json;
using Squidex.Hosting;

namespace Notifo.Infrastructure.Messaging.RabbitMq
{
    public sealed class RabbitMqProducer<T> : IAbstractProducer<T>, IInitializable
    {
        private readonly RabbitMqPrimitiveProducer primitivePublisher;
        private readonly string queueName;
        private readonly IJsonSerializer serializer;

        public string Name => $"RabbitMqProducer({queueName})";

        public RabbitMqProducer(RabbitMqPrimitiveProducer primitivePublisher, string queueName, IJsonSerializer serializer)
        {
            this.primitivePublisher = primitivePublisher;
            this.queueName = queueName;
            this.serializer = serializer;
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            primitivePublisher.CreateQueue(queueName);

            return Task.CompletedTask;
        }

        public Task ProduceAsync(string key, T message)
        {
            var bytes = serializer.SerializeToBytes(message);

            primitivePublisher.Publish(queueName, bytes);

            return Task.CompletedTask;
        }
    }
}
