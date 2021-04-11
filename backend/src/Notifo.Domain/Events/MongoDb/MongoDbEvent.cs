// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Text;
using Microsoft.Extensions.ObjectPool;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Events.MongoDb
{
    public sealed class MongoDbEvent : MongoDbEntity<Event>
    {
        private static readonly ObjectPool<StringBuilder> StringBuilderPool = ObjectPool.Create(new StringBuilderPooledObjectPolicy());

        public string SearchText { get; set; }

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
            var sb = StringBuilderPool.Get();
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
                StringBuilderPool.Return(sb);
            }
        }

        public Event ToEvent()
        {
            return Doc;
        }
    }
}
