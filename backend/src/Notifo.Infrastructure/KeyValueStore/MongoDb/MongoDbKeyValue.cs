// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;

namespace Notifo.Infrastructure.KeyValueStore.MongoDb;

public sealed class MongoDbKeyValue
{
    [BsonId]
    [BsonElement]
    public string Key { get; set; }

    [BsonIgnoreIfDefault]
    [BsonElement("v")]
    public string? Value { get; set; }
}
