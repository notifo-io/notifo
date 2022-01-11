// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
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
        private readonly TimeSpan retentionTime;

        static MongoDbEventRepository()
        {
            BsonClassMap.RegisterClassMap<MongoDbEvent>(cm =>
            {
                cm.AutoMap();

                cm.SetIgnoreExtraElements(true);

                cm.SetIdMember(null);
            });
        }

        public MongoDbEventRepository(IMongoDatabase database, IOptions<EventsOptions> options)
            : base(database)
        {
            retentionTime = options.Value.RetentionTime;
        }

        protected override string CollectionName()
        {
            return "Events";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoDbEvent> collection,
            CancellationToken ct = default)
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
                    new CreateIndexOptions
                    {
                        ExpireAfter = retentionTime
                    }),
                null, ct);
        }

        public async Task<IResultList<Event>> QueryAsync(string appId, EventQuery query,
            CancellationToken ct = default)
        {
            using (var activity = Telemetry.Activities.StartActivity("MongoDbEventRepository/QueryAsync"))
            {
                var filters = new List<FilterDefinition<MongoDbEvent>>
                {
                    Filter.Eq(x => x.Doc.AppId, appId)
                };

                if (!string.IsNullOrWhiteSpace(query.Query))
                {
                    var regex = new BsonRegularExpression(Regex.Escape(query.Query), "i");

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

                activity?.SetTag("numResults", resultItems.Count);
                activity?.SetTag("numTotal", resultTotal);

                return ResultList.Create(resultTotal, resultItems.Select(x => x.ToEvent()));
            }
        }

        public async Task InsertAsync(Event @event,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("MongoDbEventRepository/InsertAsync"))
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
        }

        public async Task BatchWriteAsync(List<((string AppId, string EventId) Key, CounterMap Counters)> counters,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoDbEventRepository/BatchWriteAsync"))
            {
                var writes = new List<WriteModel<MongoDbEvent>>(counters.Count);

                foreach (var ((appId, id), values) in counters)
                {
                    if (values?.Count > 0)
                    {
                        var docId = MongoDbEvent.CreateId(appId, id);

                        var updates = new List<UpdateDefinition<MongoDbEvent>>(values.Count);

                        foreach (var (key, value) in values)
                        {
                            updates.Add(Update.Inc($"d.Counters.{key}", value));
                        }

                        var model = new UpdateOneModel<MongoDbEvent>(Filter.Eq(x => x.DocId, docId), Update.Combine(updates))
                        {
                            IsUpsert = true
                        };

                        writes.Add(model);
                    }
                }

                if (writes.Count > 0)
                {
                    await Collection.BulkWriteAsync(writes, cancellationToken: ct);
                }
            }
        }
    }
}
