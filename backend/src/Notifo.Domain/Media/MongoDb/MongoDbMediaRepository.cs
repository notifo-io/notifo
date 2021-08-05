// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Notifo.Infrastructure;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Media.MongoDb
{
    public sealed class MongoDbMediaRepository : MongoDbStore<MongoDbMedia>, IMediaRepository
    {
        public MongoDbMediaRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Media";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoDbMedia> collection,
            CancellationToken ct)
        {
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoDbMedia>(
                    IndexKeys
                        .Ascending(x => x.Doc.AppId)
                        .Ascending(x => x.Doc.FileName)),
                null, ct);
        }

        public async Task<IResultList<Media>> QueryAsync(string appId, MediaQuery query,
            CancellationToken ct)
        {
            var filters = new List<FilterDefinition<MongoDbMedia>>
            {
                Filter.Eq(x => x.Doc.AppId, appId)
            };

            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                var regex = new BsonRegularExpression(Regex.Escape(query.Query), "i");

                filters.Add(Filter.Regex(x => x.Doc.FileName, regex));
            }

            var filter = Filter.And(filters);

            var resultItems = await Collection.Find(filter).ToListAsync(query, ct);
            var resultTotal = (long)resultItems.Count;

            if (query.ShouldQueryTotal(resultItems))
            {
                resultTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
            }

            return ResultList.Create(resultTotal, resultItems.Select(x => x.ToMedia()));
        }

        public async Task<Media?> GetAsync(string appId, string fileName,
            CancellationToken ct)
        {
            var docId = MongoDbMedia.CreateId(appId, fileName);

            var document = await Collection.Find(x => x.DocId == docId).FirstOrDefaultAsync(ct);

            return document?.ToMedia();
        }

        public Task UpsertAsync(Media media,
            CancellationToken ct)
        {
            var document = MongoDbMedia.FromMedia(media);

            return Collection.ReplaceOneAsync(x => x.DocId == document.DocId, document, UpsertReplace, ct);
        }

        public Task DeleteAsync(string appId, string fileName,
            CancellationToken ct)
        {
            var docId = MongoDbMedia.CreateId(appId, fileName);

            return Collection.DeleteOneAsync(x => x.DocId == docId, ct);
        }
    }
}
