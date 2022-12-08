// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NodaTime;
using Notifo.Infrastructure;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Log.MongoDb;

public sealed class MongoDbLogRepository : MongoDbStore<MongoDbLogEntry>, ILogRepository
{
    static MongoDbLogRepository()
    {
        BsonClassMap.RegisterClassMap<LogEntry>(cm =>
        {
            cm.AutoMap();

            cm.MapProperty(x => x.UserId)
                .SetIgnoreIfNull(true);
        });
    }

    public MongoDbLogRepository(IMongoDatabase database)
        : base(database)
    {
    }

    protected override string CollectionName()
    {
        return "Log";
    }

    protected override async Task SetupCollectionAsync(IMongoCollection<MongoDbLogEntry> collection,
        CancellationToken ct = default)
    {
        await collection.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<MongoDbLogEntry>(
                IndexKeys
                    .Ascending(x => x.Doc.AppId)
                    .Ascending(x => x.Doc.UserId)),
            new CreateIndexModel<MongoDbLogEntry>(
                IndexKeys
                    .Ascending(x => x.Doc.FirstWriteId)),
        }, null, ct);
    }

    public async Task<IResultList<LogEntry>> QueryAsync(string appId, LogQuery query,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("MongoDbLogRepository/DeleteAsync"))
        {
            var filter = BuildFilter(appId, query);

            var resultItems = await Collection.Find(filter).SortByDescending(x => x.Doc.LastSeen).ToListAsync(query, ct);
            var resultTotal = (long)resultItems.Count;

            if (query.ShouldQueryTotal(resultItems))
            {
                resultTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
            }

            activity?.SetTag("numResults", resultItems.Count);
            activity?.SetTag("numTotal", resultTotal);

            return ResultList.Create(resultTotal, resultItems.Select(x => x.ToEntry()));
        }
    }

    public async Task<IResultList<LogEntry>> BatchWriteAsync(IEnumerable<(LogWrite Write, int Count, Instant Now)> updates,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("MongoDbLogRepository/BatchWriteAsync"))
        {
            // Use a token to track which entries have just been written.
            var writeId = Guid.NewGuid().ToString();
            var writes = new List<WriteModel<MongoDbLogEntry>>();

            foreach (var (write, count, now) in updates)
            {
                var (appId, userId, eventCode, message, system) = write;
                var docId = MongoDbLogEntry.CreateId(appId, userId, eventCode, message, system);

                var update =
                    Update
                        .SetOnInsert(x => x.DocId, docId)
                        .SetOnInsert(x => x.Doc.AppId, appId)
                        .SetOnInsert(x => x.Doc.EventCode, eventCode)
                        .SetOnInsert(x => x.Doc.FirstSeen, now)
                        .SetOnInsert(x => x.Doc.FirstWriteId, writeId)
                        .SetOnInsert(x => x.Doc.Message, message)
                        .SetOnInsert(x => x.Doc.UserId, userId)
                        .SetOnInsert(x => x.Doc.System, system)
                        .Set(x => x.Doc.LastSeen, now)
                        .Inc(x => x.Doc.Count, count);

                var model = new UpdateOneModel<MongoDbLogEntry>(Filter.Eq(x => x.DocId, docId), update)
                {
                    IsUpsert = true
                };

                writes.Add(model);
            }

            await Collection.BulkWriteAsync(writes, cancellationToken: ct);

            // Every log enty with the first write token has been just created.
            var resultItems = await Collection.Find(x => x.Doc.FirstWriteId == writeId).ToListAsync(ct);
            var resultTotal = (long)resultItems.Count;

            activity?.SetTag("numNewEntries", resultItems.Count);

            return ResultList.Create(resultTotal, resultItems.Select(x => x.ToEntry()));
        }
    }

    private static FilterDefinition<MongoDbLogEntry> BuildFilter(string appId, LogQuery query)
    {
        var filters = new List<FilterDefinition<MongoDbLogEntry>>
        {
            Filter.Eq(x => x.Doc.AppId, appId)
        };

        if (!string.IsNullOrWhiteSpace(query.UserId))
        {
            filters.Add(Filter.Eq(x => x.Doc.UserId, query.UserId));
        }
        else
        {
            filters.Add(Filter.Or(Filter.Exists(x => x.Doc.UserId, false), Filter.Eq(x => x.Doc.UserId, null)));
        }

        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var regex = new BsonRegularExpression(Regex.Escape(query.Query), "i");

            filters.Add(Filter.Regex(x => x.Doc.Message, regex));
        }

        if (query.Systems?.Length > 0)
        {
            var ors = new List<FilterDefinition<MongoDbLogEntry>>()
            {
                Filter.In(x => x.Doc.System, query.Systems)
            };

            foreach (var system in query.Systems)
            {
                ors.Add(Filter.Regex(x => x.Doc.System, new BsonRegularExpression($"^{system}:", "i")));
            }

            filters.Add(Filter.Or(ors));
        }

        if (query.EventCode > 0)
        {
            filters.Add(Filter.Eq(x => x.Doc.EventCode, query.EventCode));
        }

        return Filter.And(filters);
    }
}
