// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Notifo.Infrastructure.MongoDb
{
    public sealed class SoftEnumSerializer<T> : SerializerBase<T>, IBsonPolymorphicSerializer where T : struct
    {
        private static volatile int isRegistered;

        public static void Register()
        {
            if (Interlocked.Increment(ref isRegistered) == 1)
            {
                BsonSerializer.RegisterSerializer(new SoftEnumSerializer<T>());
            }
        }

        public bool IsDiscriminatorCompatibleWithObjectSerializer
        {
            get => true;
        }

        public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;

            if (reader.CurrentBsonType == BsonType.Null)
            {
                reader.ReadNull();
                return default;
            }

            if (reader.CurrentBsonType == BsonType.Int32)
            {
                var number = reader.ReadInt32();

                return Unsafe.As<int, T>(ref number);
            }

            if (reader.CurrentBsonType == BsonType.Int64)
            {
                var number = (int)reader.ReadInt64();

                return Unsafe.As<int, T>(ref number);
            }

            var text = context.Reader.ReadString();

            if (Enum.TryParse<T>(text, true, out var confirmMode))
            {
                return confirmMode;
            }

            return default;
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
        {
            context.Writer.WriteString(value.ToString());
        }
    }
}
