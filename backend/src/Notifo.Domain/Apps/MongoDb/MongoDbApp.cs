// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using Notifo.Domain.Integrations;
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

        [BsonIgnoreIfDefault]
        public bool IsPending { get; set; }

        public static MongoDbApp FromApp(App app)
        {
            var result = new MongoDbApp
            {
                DocId = app.Id,
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

            result.IsPending = app.Integrations.Values.Any(x => x.Status == IntegrationStatus.Pending);

            return result;
        }

        public App ToApp()
        {
            return Doc;
        }
    }
}
