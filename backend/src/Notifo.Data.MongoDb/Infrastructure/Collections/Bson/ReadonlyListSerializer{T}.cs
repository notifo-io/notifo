// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Notifo.Infrastructure.Collections.Bson;

public sealed class ReadonlyListSerializer<T> : ClassSerializerBase<ReadonlyList<T>>
{
    private readonly Type innerType = typeof(List<T>);

    protected override ReadonlyList<T> DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var inner = BsonSerializer.Deserialize<List<T>>(context.Reader);

        return new ReadonlyList<T>(inner);
    }

    protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, ReadonlyList<T> value)
    {
        var inner = new List<T>(value);

        BsonSerializer.Serialize(context.Writer, innerType, inner);
    }
}
