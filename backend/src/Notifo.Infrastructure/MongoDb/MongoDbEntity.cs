// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;

namespace Notifo.Infrastructure.MongoDb
{
    public abstract class MongoDbEntity
    {
        [BsonId]
        [BsonElement]
        public string DocId { get; set; }

        [BsonElement("e")]
        [BsonIgnoreIfNull]
        public string Etag { get; set; }

        public static string GenerateEtag()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);
        }
    }

    [BsonIgnoreExtraElements]
    public abstract class MongoDbEntity<T> : MongoDbEntity
    {
        [BsonElement("d")]
        [BsonRequired]
        public T Doc { get; set; }
    }
}
