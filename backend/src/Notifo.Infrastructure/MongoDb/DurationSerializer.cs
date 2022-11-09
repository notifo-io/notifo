// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NodaTime;
using NodaTime.Text;

namespace Notifo.Infrastructure.MongoDb;

public sealed class DurationSerializer : SerializerBase<Duration>, IBsonPolymorphicSerializer
{
    private static volatile int isRegistered;

    public static void Register()
    {
        if (Interlocked.Increment(ref isRegistered) == 1)
        {
            BsonSerializer.RegisterSerializer(new DurationSerializer());
        }
    }

    public bool IsDiscriminatorCompatibleWithObjectSerializer
    {
        get => true;
    }

    public override Duration Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var reader = context.Reader;

        if (reader.CurrentBsonType == BsonType.Document)
        {
            reader.ReadStartDocument();
            reader.ReadEndDocument();
            return default;
        }

        var text = reader.ReadString();

        return DurationPattern.JsonRoundtrip.Parse(text).Value;
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Duration value)
    {
        context.Writer.WriteString(DurationPattern.JsonRoundtrip.Format(value));
    }
}
