// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Notifo.Infrastructure;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Templates.MongoDb
{
    public sealed class MongoDbTemplateRepository : MongoDbStore<MongoDbTemplate>, ITemplateRepository
    {
        public MongoDbTemplateRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Templates";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoDbTemplate> collection, CancellationToken ct)
        {
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoDbTemplate>(
                    IndexKeys
                        .Ascending(x => x.Doc.AppId)
                        .Ascending(x => x.Doc.Code),
                    null),
                null, ct);
        }

        public async Task<IResultList<Template>> QueryAsync(string appId, TemplateQuery query, CancellationToken ct)
        {
            var filters = new List<FilterDefinition<MongoDbTemplate>>
            {
                Filter.Eq(x => x.Doc.AppId, appId)
            };

            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                var regex = new BsonRegularExpression(query.Query, "i");

                filters.Add(Filter.Regex(x => x.Doc.Code, regex));
            }

            var filter = Filter.And(filters);

            var resultItems = await Collection.Find(filter).ToListAsync(query, ct);
            var resultTotal = (long)resultItems.Count;

            if (resultTotal >= query.Take || query.Skip > 0)
            {
                resultTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
            }

            return ResultList.Create(resultTotal, resultItems.Select(x => x.ToTemplate()));
        }

        public async Task<(Template? Template, string? Etag)> GetAsync(string appId, string code, CancellationToken ct)
        {
            var docId = MongoDbTemplate.CreateId(appId, code);

            var document = await GetDocumentAsync(docId, ct);

            return (document?.ToTemplate(), document?.Etag);
        }

        public Task UpsertAsync(Template template, string? oldEtag, CancellationToken ct)
        {
            var document = MongoDbTemplate.FromTemplate(template);

            return UpsertDocumentAsync(document.DocId, document, oldEtag, ct);
        }

        public Task DeleteAsync(string appId, string id, CancellationToken ct)
        {
            var docId = MongoDbTemplate.CreateId(appId, id);

            return Collection.DeleteOneAsync(x => x.DocId == docId, ct);
        }
    }
}
