// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Notifo.Domain.UserNotifications.MongoDb;

internal sealed class RandomGuidSerializer : SerializerBase<Guid>
{
    public override Guid Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var read = context.Reader.ReadString();

        if (!Guid.TryParse(read, out var id))
        {
            id = Guid.NewGuid();
        }

        return id;
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Guid value)
    {
        var writer = context.Writer;

        writer.WriteString(value.ToString());
    }
}
