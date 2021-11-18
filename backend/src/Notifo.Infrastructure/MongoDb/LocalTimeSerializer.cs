// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NodaTime;
using NodaTime.Text;

namespace Notifo.Infrastructure.MongoDb
{
    public sealed class LocalTimeSerializer : SerializerBase<LocalTime>, IBsonPolymorphicSerializer
    {
        private static volatile int isRegistered;

        public static void Register()
        {
            if (Interlocked.Increment(ref isRegistered) == 1)
            {
                BsonSerializer.RegisterSerializer(new LocalTimeSerializer());
            }
        }

        public bool IsDiscriminatorCompatibleWithObjectSerializer
        {
            get => true;
        }

        public override LocalTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var value = context.Reader.ReadString();

            return LocalTimePattern.GeneralIso.Parse(value).Value;
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, LocalTime value)
        {
            context.Writer.WriteString(LocalTimePattern.GeneralIso.Format(value));
        }
    }
}
