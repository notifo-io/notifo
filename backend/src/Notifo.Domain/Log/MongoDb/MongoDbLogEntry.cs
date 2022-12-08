// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Log.MongoDb;

public sealed class MongoDbLogEntry : MongoDbEntity
{
    [BsonElement("Entry")]
    [BsonRequired]
    public LogEntry Doc { get; set; }

    public static string CreateId(string appId, string? userId, int eventCode, string message, string system)
    {
        return $"{appId}_{userId}_{eventCode}_{message}_{system}";
    }

    public LogEntry ToEntry()
    {
        var entry = Doc;

        if (string.IsNullOrWhiteSpace(entry.System))
        {
            var indexOfDot = entry.Message.IndexOf(':', StringComparison.OrdinalIgnoreCase);

            if (indexOfDot > 0)
            {
                entry.System = entry.Message.Substring(0, indexOfDot);
            }
        }

        return entry;
    }
}
