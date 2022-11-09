// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;

namespace Notifo.Identity.MongoDb;

public sealed class MongoDbKey
{
    [BsonId]
    public string Id { get; set; }

    [BsonElement]
    public string Key { get; set; }

    [BsonElement]
    public MongoDbKeyParameters Parameters { get; set; }
}
