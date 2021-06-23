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
using NodaTime;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Topics.MongoDb
{
    public sealed class MongoDbTopicRepository : MongoDbRepository<MongoDbTopic>, ITopicRepository
    {
        private readonly IClock clock;

        public MongoDbTopicRepository(IMongoDatabase database, IClock clock)
            : base(database)
        {
            this.clock = clock;
        }

        protected override string CollectionName()
        {
            return "Topics";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoDbTopic> collection, CancellationToken ct)
        {
            await Collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoDbTopic>(
                    IndexKeys
                        .Ascending(x => x.Doc.AppId)
                        .Ascending(x => x.Doc.Path)
                        .Descending(x => x.Doc.LastUpdate)),
                null, ct);
        }

        public async Task<IResultList<Topic>> QueryAsync(string appId, TopicQuery query, CancellationToken ct)
        {
            var filters = new List<FilterDefinition<MongoDbTopic>>
            {
                Filter.Eq(x => x.Doc.AppId, appId)
            };

            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                var regex = new BsonRegularExpression(Regex.Escape(query.Query), "i");

                filters.Add(Filter.Regex(x => x.Doc.Path, regex));
            }

            var filter = Filter.And(filters);

            var resultItems = await Collection.Find(filter).SortByDescending(x => x.Doc.LastUpdate).ToListAsync(query, ct);
            var resultTotal = (long)resultItems.Count;

            if (query.ShouldQueryTotal(resultItems))
            {
                resultTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
            }

            return ResultList.Create(resultTotal, resultItems.Select(x => x.ToTopic()));
        }

        public Task BatchWriteAsync(List<((string AppId, string Path) Key, CounterMap Counters)> counters, CancellationToken ct)
        {
            var writes = new List<WriteModel<MongoDbTopic>>();

            var now = clock.GetCurrentInstant();

            foreach (var ((appId, path), appCounters) in counters)
            {
                var docId = MongoDbTopic.CreateId(appId, path);

                var updates = new List<UpdateDefinition<MongoDbTopic>>
                {
                    Update.Set(x => x.Doc.LastUpdate, now),
                    Update.SetOnInsert(x => x.Doc.AppId, appId),
                    Update.SetOnInsert(x => x.Doc.Path, path)
                };

                foreach (var (key, value) in appCounters)
                {
                    updates.Add(Update.Inc($"Counters.{key}", value));
                }

                var model = new UpdateOneModel<MongoDbTopic>(Filter.Eq(x => x.DocId, docId), Update.Combine(updates));

                writes.Add(model);
            }

            return Collection.BulkWriteAsync(writes, cancellationToken: ct);
        }
    }
}
