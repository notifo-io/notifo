﻿// ==========================================================================
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

namespace Notifo.Domain.ChannelTemplates.MongoDb
{
    public sealed class MongoDbChannelTemplateRepository<T> : MongoDbStore<MongoDbChannelTemplate<T>>, IChannelTemplateRepository<T> where T : class
    {
        public MongoDbChannelTemplateRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return $"ChannelTemplates_{typeof(T).Name}";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoDbChannelTemplate<T>> collection,
            CancellationToken ct)
        {
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoDbChannelTemplate<T>>(
                    IndexKeys
                        .Ascending(x => x.Doc.AppId)
                        .Ascending(x => x.Doc.Id)),
                null, ct);
        }

        public async Task<IResultList<ChannelTemplate<T>>> QueryAsync(string appId, ChannelTemplateQuery query,
            CancellationToken ct)
        {
            using (var activity = Telemetry.Activities.StartMethod<MongoDbChannelTemplateRepository<T>>())
            {
                var filters = new List<FilterDefinition<MongoDbChannelTemplate<T>>>
                {
                    Filter.Eq(x => x.Doc.AppId, appId)
                };

                if (!string.IsNullOrWhiteSpace(query.Query))
                {
                    var regex = new BsonRegularExpression(Regex.Escape(query.Query), "i");

                    filters.Add(Filter.Regex(x => x.Doc.Name, regex));
                }

                var filter = Filter.And(filters);

                var resultItems = await Collection.Find(filter).ToListAsync(query, ct);
                var resultTotal = (long)resultItems.Count;

                if (query.ShouldQueryTotal(resultItems))
                {
                    resultTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
                }

                activity?.SetTag("numResults", resultItems.Count);
                activity?.SetTag("numTotal", resultTotal);

                return ResultList.Create(resultTotal, resultItems.Select(x => x.ToChannelTemplate()));
            }
        }

        public async Task<ChannelTemplate<T>?> GetBestAsync(string appId, string? name,
            CancellationToken ct )
        {
            using (Telemetry.Activities.StartMethod<MongoDbChannelTemplateRepository<T>>())
            {
                var templates =
                    await Collection.Find(x => x.Doc.AppId == appId)
                        .Project<MongoDbChannelTemplate<T>>(
                            Projection
                                .Include(x => x.DocId)
                                .Include(x => x.Doc.Name)
                                .Include(x => x.Doc.Primary))
                        .ToListAsync(ct);

                string? id = null;

                if (!string.IsNullOrWhiteSpace(name))
                {
                    id = templates.Find(x => x.Doc.Name == name)?.DocId;
                }
                else
                {
                    id = templates.Find(x => x.Doc.Primary)?.DocId;

                    if (id == null && templates.Count == 1)
                    {
                        id = templates[0].DocId;
                    }
                }

                if (id == null)
                {
                    return null;
                }

                var document = await Collection.Find(x => x.DocId == id).FirstOrDefaultAsync(ct);

                return document?.ToChannelTemplate();
            }
        }

        public async Task<(ChannelTemplate<T>? Template, string? Etag)> GetAsync(string appId, string code,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartMethod<MongoDbChannelTemplateRepository<T>>())
            {
                var docId = MongoDbChannelTemplate<T>.CreateId(appId, code);

                var document = await GetDocumentAsync(docId, ct);

                return (document?.ToChannelTemplate(), document?.Etag);
            }
        }

        public Task UpsertAsync(ChannelTemplate<T> template, string? oldEtag,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartMethod<MongoDbChannelTemplateRepository<T>>())
            {
                var document = MongoDbChannelTemplate<T>.FromChannelTemplate(template);

                return UpsertDocumentAsync(document.DocId, document, oldEtag, ct);
            }
        }

        public Task DeleteAsync(string appId, string id,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartMethod<MongoDbChannelTemplateRepository<T>>())
            {
                var docId = MongoDbChannelTemplate<T>.CreateId(appId, id);

                return Collection.DeleteOneAsync(x => x.DocId == docId, ct);
            }
        }
    }
}
