// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Apps.MongoDb
{
    [BsonIgnoreExtraElements]
    public sealed class MongoDbApp : MongoDbEntity<App>
    {
        [BsonIgnoreIfNull]
        public List<string> ContributorIds { get; set; }

        [BsonIgnoreIfNull]
        public List<string> ApiKeys { get; set; }

        public static MongoDbApp FromApp(App app)
        {
            var id = app.Id;

            var result = new MongoDbApp
            {
                DocId = id,
                Doc = app,
                Etag = GenerateEtag()
            };

            if (app.Contributors?.Count > 0)
            {
                result.ContributorIds = app.Contributors.Keys.ToList();
            }

            if (app.ApiKeys?.Count > 0)
            {
                result.ApiKeys = app.ApiKeys.Keys.ToList();
            }

            return result;
        }

        public App ToApp()
        {
            return Doc;
        }
    }
}
