// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Events.MongoDb
{
    public sealed class MongoDbEventRepository : MongoDbStore<MongoDbEvent>, IEventRepository
    {
        static MongoDbEventRepository()
        {
            BsonClassMap.RegisterClassMap<MongoDbEvent>(cm =>
            {
                cm.AutoMap();

                cm.SetIgnoreExtraElements(true);

                cm.SetIdMember(null);
            });
        }

        public MongoDbEventRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Events";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoDbEvent> collection, CancellationToken ct)
        {
            await Collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoDbEvent>(
                    IndexKeys
                        .Ascending(x => x.Doc.AppId)
                        .Ascending(x => x.Doc.Topic)
                        .Ascending(x => x.SearchText)
                        .Descending(x => x.Doc.Created)),
                null, ct);

            await Collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoDbEvent>(
                    IndexKeys
                        .Descending(x => x.Doc.Created),
                    new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(10) }),
                null, ct);
        }

        public async Task<IResultList<Event>> QueryAsync(string appId, EventQuery query, CancellationToken ct)
        {
            var filters = new List<FilterDefinition<MongoDbEvent>>
            {
                Filter.Eq(x => x.Doc.AppId, appId)
            };

            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                var regex = new BsonRegularExpression(query.Query, "i");

                filters.Add(
                    Filter.Or(
                        Filter.Regex(x => x.Doc.Topic, regex),
                        Filter.Regex(x => x.SearchText, regex)));
            }

            var filter = Filter.And(filters);

            var resultItems = await Collection.Find(filter).SortByDescending(x => x.Doc.Created).ToListAsync(query, ct);
            var resultTotal = (long)resultItems.Count;

            if (query.ShouldQueryTotal(resultItems))
            {
                resultTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
            }

            return ResultList.Create(resultTotal, resultItems.Select(x => x.ToEvent()));
        }

        public async Task InsertAsync(Event @event, CancellationToken ct)
        {
            var document = MongoDbEvent.FromEvent(@event);

            try
            {
                await Collection.InsertOneAsync(document, null, ct);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                throw new UniqueConstraintException();
            }
        }

        public Task BatchWriteAsync(List<((string AppId, string EventId) Key, CounterMap Counters)> counters, CancellationToken ct)
        {
            var writes = new List<WriteModel<MongoDbEvent>>();

            foreach (var ((appId, id), values) in counters)
            {
                if (values.Any())
                {
                    var docId = MongoDbEvent.CreateId(appId, id);

                    var updates = new List<UpdateDefinition<MongoDbEvent>>();

                    foreach (var (key, value) in values)
                    {
                        updates.Add(Update.Inc($"d.Counters.{key}", value));
                    }

                    var model = new UpdateOneModel<MongoDbEvent>(Filter.Eq(x => x.DocId, docId), Update.Combine(updates));

                    writes.Add(model);
                }
            }

            return Collection.BulkWriteAsync(writes, cancellationToken: ct);
        }
    }
}
