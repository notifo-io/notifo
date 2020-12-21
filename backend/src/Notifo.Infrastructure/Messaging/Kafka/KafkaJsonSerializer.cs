// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Confluent.Kafka;
using Notifo.Infrastructure.Json;

namespace Notifo.Infrastructure.Messaging.Kafka
{
    public sealed class KafkaJsonSerializer<T> : ISerializer<T>, IDeserializer<T>
    {
        private readonly IJsonSerializer serializer;

        public KafkaJsonSerializer(IJsonSerializer serializer)
        {
            this.serializer = serializer;
        }

        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            return serializer.Deserialize<T>(data);
        }

        public byte[] Serialize(T data, SerializationContext context)
        {
            return serializer.SerializeToBytes(data);
        }
    }
}
