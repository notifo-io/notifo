// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Log.MongoDb
{
    public sealed class MongoDbLogEntry : MongoDbEntity
    {
        [BsonRequired]
        public LogEntry Entry { get; set; }

        public static string CreateId(string appId, string message)
        {
            return $"{appId}_{message}";
        }

        public static MongoDbLogEntry FromMedia(LogEntry entry, string etag)
        {
            var id = CreateId(entry.AppId, entry.Message);

            var result = new MongoDbLogEntry { DocId = id, Entry = entry, Etag = etag };

            return result;
        }

        public LogEntry ToEntry()
        {
            return Entry;
        }
    }
}
