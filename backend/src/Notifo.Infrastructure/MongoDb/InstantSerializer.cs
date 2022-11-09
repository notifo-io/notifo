// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NodaTime;

namespace Notifo.Infrastructure.MongoDb;

public sealed class InstantSerializer : SerializerBase<Instant>, IBsonPolymorphicSerializer
{
    private static volatile int isRegistered;

    public static void Register()
    {
        if (Interlocked.Increment(ref isRegistered) == 1)
        {
            BsonSerializer.RegisterSerializer(new InstantSerializer());
        }
    }

    public bool IsDiscriminatorCompatibleWithObjectSerializer
    {
        get => true;
    }

    public override Instant Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var text = context.Reader.ReadDateTime();

        return Instant.FromUnixTimeMilliseconds(text);
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Instant value)
    {
        context.Writer.WriteDateTime(value.ToUnixTimeMilliseconds());
    }
}
