// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Notifo.Domain.Channels;
using Notifo.Infrastructure;

namespace Notifo.Domain.UserNotifications.MongoDb;

public sealed class StatusDictionarySerializer : ClassSerializerBase<Dictionary<Guid, ChannelSendInfo>>
{
    protected override Dictionary<Guid, ChannelSendInfo> DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var deserialized = BsonSerializer.Deserialize<Dictionary<string, ChannelSendInfo>>(context.Reader);

        var result = new Dictionary<Guid, ChannelSendInfo>();

        foreach (var (key, value) in deserialized)
        {
            value.Configuration ??= new SendConfiguration();

            if (Guid.TryParse(key, out var parsedId))
            {
                result[parsedId] = value;
            }
            else
            {
                value.Configuration[Constants.Convertedkey] = key.FromOptionalBase64();
                result[Guid.NewGuid()] = value;
            }
        }

        return result;
    }

    protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, Dictionary<Guid, ChannelSendInfo> value)
    {
        var converted = value.ToDictionary(x => x.Key.ToString(), x => x.Value);

        BsonSerializer.Serialize(context.Writer, converted);
    }
}
