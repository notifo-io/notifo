// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using NodaTime;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Topics.MongoDb;

public sealed class MongoDbTopicRepository : MongoDbStore<MongoDbTopic>, ITopicRepository
{
    public MongoDbTopicRepository(IMongoDatabase database)
        : base(database)
    {
    }

    protected override string CollectionName()
    {
        return "Topics";
    }

    protected override async Task SetupCollectionAsync(IMongoCollection<MongoDbTopic> collection,
        CancellationToken ct = default)
    {
        await Collection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoDbTopic>(
                IndexKeys
                    .Ascending(x => x.Doc.AppId)
                    .Ascending(x => x.Doc.IsExplicit)
                    .Ascending(x => x.Doc.Path)
                    .Descending(x => x.Doc.LastUpdate)),
            null, ct);
    }

    public async Task<IResultList<Topic>> QueryAsync(string appId, TopicQuery query,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("MongoDbTopicRepository/QueryAsync"))
        {
            var filter = BuildFilter(appId, query);

            var resultItems = await Collection.Find(filter).SortByDescending(x => x.Doc.LastUpdate).ToListAsync(query, ct);
            var resultTotal = (long)resultItems.Count;

            if (query.ShouldQueryTotal(resultItems))
            {
                resultTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
            }

            activity?.SetTag("numResults", resultItems.Count);
            activity?.SetTag("numTotal", resultTotal);

            return ResultList.Create(resultTotal, resultItems.Select(x => x.ToTopic()));
        }
    }

    public async Task<(Topic? Topic, string? Etag)> GetAsync(string appId, TopicId path,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbTopicRepository/GetAsync"))
        {
            var docId = MongoDbTopic.CreateId(appId, path);

            var document = await GetDocumentAsync(docId, ct);

            return (document?.ToTopic(), document?.Etag);
        }
    }

    public async Task UpsertAsync(Topic topic, string? oldEtag = null,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbTopicRepository/UpsertAsync"))
        {
            var document = MongoDbTopic.FromTopic(topic);

            await UpsertDocumentAsync(document.DocId, document, oldEtag, ct);
        }
    }

    public async Task DeleteAsync(string appId, TopicId path,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbTopicRepository/DeleteAsync"))
        {
            var docId = MongoDbTopic.CreateId(appId, path);

            await Collection.DeleteOneAsync(x => x.DocId == docId, ct);
        }
    }

    public async Task BatchWriteAsync(List<((string AppId, string Path) Key, CounterMap Counters)> counters,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoDbTopicRepository/BatchWriteAsync"))
        {
            var writes = new List<WriteModel<MongoDbTopic>>(counters.Count);

            var now = SystemClock.Instance.GetCurrentInstant();

            foreach (var ((appId, path), values) in counters)
            {
                if (values?.Count > 0)
                {
                    var docId = MongoDbTopic.CreateId(appId, path);

                    var updates = new List<UpdateDefinition<MongoDbTopic>>(values.Count);

                    foreach (var (key, value) in values)
                    {
                        updates.Add(Update
                            .SetOnInsert(x => x.Doc.AppId, appId)
                            .SetOnInsert(x => x.Doc.Path, path)
                            .SetOnInsert(x => x.Doc.Created, now)
                            .SetOnInsert(x => x.Doc.LastUpdate, now)
                            .Inc($"d.Counters.{key}", value));
                    }

                    var model = new UpdateOneModel<MongoDbTopic>(Filter.Eq(x => x.DocId, docId), Update.Combine(updates))
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

    private static FilterDefinition<MongoDbTopic> BuildFilter(string appId, TopicQuery query)
    {
        var filters = new List<FilterDefinition<MongoDbTopic>>
        {
            Filter.Eq(x => x.Doc.AppId, appId)
        };

        switch (query.Scope)
        {
            case TopicQueryScope.Explicit:
                filters.Add(Filter.Eq(x => x.Doc.IsExplicit, true));
                break;
            case TopicQueryScope.Implicit:
                filters.Add(Filter.Ne(x => x.Doc.IsExplicit, true));
                break;
            default:
                filters.Add(Filter.Ne("d.isExplicit", 0));
                break;
        }

        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var regex = new BsonRegularExpression(Regex.Escape(query.Query), "i");

            filters.Add(Filter.Regex(x => x.Doc.Path, regex));
        }

        return Filter.And(filters);
    }
}
