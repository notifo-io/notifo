// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Notifo.Domain.Events.MongoDb
{
    public sealed class NotificationSendSerializer : SerializerBase<NotificationSend>
    {
        private static volatile int isRegistered;

        public static void Register()
        {
            if (Interlocked.Increment(ref isRegistered) == 1)
            {
                BsonSerializer.RegisterSerializer(new NotificationSendSerializer());
            }
        }

        public override NotificationSend Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;

            switch (reader.CurrentBsonType)
            {
                case BsonType.Null:
                    reader.ReadNull();
                    return NotificationSend.Inherit;
                case BsonType.Boolean:
                    {
                        var value = reader.ReadBoolean();

                        return value ? NotificationSend.Send : NotificationSend.NotSending;
                    }

                case BsonType.String:
                    {
                        var value = reader.ReadString();

                        if (Enum.TryParse<NotificationSend>(value, true, out var result))
                        {
                            return result;
                        }

                        break;
                    }
            }

            throw new NotSupportedException();
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, NotificationSend value)
        {
            context.Writer.WriteString(value.ToString());
        }
    }
}
