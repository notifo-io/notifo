// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Notifo.Infrastructure.MongoDb
{
    public sealed class ActivitySpanIdSerializer : SerializerBase<ActivitySpanId>
    {
        private static volatile int isRegistered;

        public static void Register()
        {
            if (Interlocked.Increment(ref isRegistered) == 1)
            {
                BsonSerializer.RegisterSerializer(new ActivitySpanIdSerializer());
            }
        }

        public override ActivitySpanId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;

            switch (reader.CurrentBsonType)
            {
                case BsonType.Null:
                    reader.ReadNull();
                    return default;
                case BsonType.String:
                    var text = context.Reader.ReadString();

                    return ActivitySpanId.CreateFromString(text);
                default:
                    ThrowHelper.BsonSerializationException($"Expected BsonType.String or JsonTokenType.Null, got {reader.CurrentBsonType}.");
                    return default;
            }
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ActivitySpanId value)
        {
            var writer = context.Writer;

            if (value == default)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteString(value.ToString());
            }
        }
    }
}
