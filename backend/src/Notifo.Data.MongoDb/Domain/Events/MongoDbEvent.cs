// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using Notifo.Infrastructure;
using Notifo.Infrastructure.ObjectPool;

namespace Notifo.Domain.Events;

public sealed class MongoDbEvent : MongoDbEntity<Event>
{
    public string SearchText { get; set; }

    [BsonIgnoreIfDefault]
    public bool Pending { get; set; }

    public static string CreateId(string appId, string id)
    {
        return $"{appId}_{id}";
    }

    public static MongoDbEvent FromEvent(Event @event)
    {
        var docId = CreateId(@event.AppId, @event.Id);

        return new MongoDbEvent
        {
            DocId = docId,
            Doc = @event,
            Etag = Guid.NewGuid().ToString(),
            SearchText = BuildSearchText(@event)
        };
    }

    private static string BuildSearchText(Event @event)
    {
        var sb = DefaultPools.StringBuilder.Get();
        try
        {
            foreach (var text in @event.Formatting.Subject.Values)
            {
                sb.AppendLine(text);
            }

            if (@event.Formatting.Body != null)
            {
                foreach (var text in @event.Formatting.Body.Values)
                {
                    sb.AppendLine(text);
                }
            }

            return sb.ToString();
        }
        finally
        {
            DefaultPools.StringBuilder.Return(sb);
        }
    }

    public Event ToEvent()
    {
        return Doc;
    }
}
