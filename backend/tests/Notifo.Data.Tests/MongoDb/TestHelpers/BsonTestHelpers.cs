// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Notifo.Infrastructure;

namespace Notifo.MongoDb.TestHelpers;

public static class BsonTestHelpers
{
    static BsonTestHelpers()
    {
        MongoClientFactory.RegisterDefaultSerializers();
    }

    public sealed class ObjectHolder<T>
    {
        [BsonRequired]
        public T Value { get; set; }
    }

    public static T SerializeAndDeserializeBson<T>(this T value)
    {
        return value.SerializeAndDeserializeBson<T, T>();
    }

    public static TOut SerializeAndDeserializeBson<TIn, TOut>(this TIn value)
    {
        var obj = new ObjectHolder<TIn>
        {
            Value = value
        };

        var stream = new MemoryStream();

        using (var writer = new BsonBinaryWriter(stream))
        {
            BsonSerializer.Serialize(writer, obj);

            writer.Flush();
        }

        stream.Position = 0;

        using (var reader = new BsonBinaryReader(stream))
        {
            var result = BsonSerializer.Deserialize<ObjectHolder<TOut>>(reader);

            return result.Value;
        }
    }
}
