// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Notifo.Infrastructure.Collections.Bson;

public sealed class ReadonlyDictionarySerializer<TKey, TValue> : ClassSerializerBase<ReadonlyDictionary<TKey, TValue>> where TKey : notnull
{
    private readonly Type innerType = typeof(Dictionary<TKey, TValue>);

    protected override ReadonlyDictionary<TKey, TValue> DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var inner = BsonSerializer.Deserialize<Dictionary<TKey, TValue>>(context.Reader);

        return new ReadonlyDictionary<TKey, TValue>(inner);
    }

    protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, ReadonlyDictionary<TKey, TValue> value)
    {
        var inner = new Dictionary<TKey, TValue>(value);

        BsonSerializer.Serialize(context.Writer, innerType, inner);
    }
}
