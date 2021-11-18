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
    public sealed class LocalDateSerializer : SerializerBase<LocalDate>, IBsonPolymorphicSerializer
    {
        private static volatile int isRegistered;

        public static void Register()
        {
            if (Interlocked.Increment(ref isRegistered) == 1)
            {
                BsonSerializer.RegisterSerializer(new LocalDateSerializer());
            }
        }

        public bool IsDiscriminatorCompatibleWithObjectSerializer
        {
            get => true;
        }

        public override LocalDate Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var value = context.Reader.ReadString();

            return LocalDatePattern.Iso.Parse(value).Value;
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, LocalDate value)
        {
            context.Writer.WriteString(LocalDatePattern.Iso.Format(value));
        }
    }
}
