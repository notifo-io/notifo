// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Notifo.Infrastructure;

namespace Notifo.Domain.Events.MongoDb
{
    public sealed class NotificationSendSerializer : SerializerBase<ChannelSend>
    {
        private static volatile int isRegistered;

        public static void Register()
        {
            if (Interlocked.Increment(ref isRegistered) == 1)
            {
                BsonSerializer.RegisterSerializer(new NotificationSendSerializer());
            }
        }

        public override ChannelSend Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;

            switch (reader.CurrentBsonType)
            {
                case BsonType.Null:
                    reader.ReadNull();
                    return ChannelSend.Inherit;
                case BsonType.Boolean:
                    {
                        var value = reader.ReadBoolean();

                        return value ? ChannelSend.Send : ChannelSend.NotSending;
                    }

                case BsonType.String:
                    {
                        var value = reader.ReadString();

                        if (Enum.TryParse<ChannelSend>(value, true, out var result))
                        {
                            return result;
                        }

                        break;
                    }
            }

            ThrowHelper.NotSupportedException();
            return default;
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ChannelSend value)
        {
            context.Writer.WriteString(value.ToString());
        }
    }
}
