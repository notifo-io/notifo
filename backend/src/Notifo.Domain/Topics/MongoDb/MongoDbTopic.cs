// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Topics.MongoDb
{
    public sealed class MongoDbTopic : MongoDbEntity<Topic>
    {
        public static string CreateId(string appId, string path)
        {
            return $"{appId}_{path}";
        }

        public Topic ToTopic()
        {
            return Doc;
        }
    }
}
