// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Apps.MongoDb;

internal sealed class MongoDbAppRepository : MongoDbStore<MongoDbApp>, IAppRepository
{
    static MongoDbAppRepository()
    {
        BsonClassMap.RegisterClassMap<App>(cm =>
        {
            cm.AutoMap();

            cm.MapProperty(x => x.Counters)
                .SetIgnoreIfNull(true);
        });
    }

    public MongoDbAppRepository(IMongoDatabase database)
        : base(database)
    {
    }

    protected override string CollectionName()
    {
        return "Apps";
    }

    protected override async Task SetupCollectionAsync(IMongoCollection<MongoDbApp> collection,
        CancellationToken ct = default)
    {
        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoDbApp>(
                IndexKeys
                    .Ascending(x => x.IsPending)),
            null, ct);

        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoDbApp>(
                IndexKeys
                    .Ascending(x => x.ContributorIds)),
            null, ct);

        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoDbApp>(
                IndexKeys
                    .Ascending(x => x.ApiKeys),
                new CreateIndexOptions { Unique = true }),
            null, ct);
    }

    public async IAsyncEnumerable<App> QueryAllAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var document in Collection.Find(new BsonDocument()).ToAsyncEnumerable(ct))
        {
            yield return document.ToApp();
        }
    }

    public async Task<List<App>> QueryWithPendingIntegrationsAsync(
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbAppRepository/QueryWithPendingIntegrationsAsync"))
        {
            var documents =
                await Collection.Find(x => x.IsPending)
                    .ToListAsync(ct);

            return documents.Select(x => x.ToApp()).ToList();
        }
    }

    public async Task<List<App>> QueryAsync(string contributorId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbAppRepository/QueryAsync"))
        {
            var documents =
                await Collection.Find(x => x.ContributorIds.Contains(contributorId))
                    .ToListAsync(ct);

            return documents.Select(x => x.ToApp()).ToList();
        }
    }

    public async Task<(App? App, string? Etag)> GetByApiKeyAsync(string apiKey,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbAppRepository/GetByApiKeyAsync"))
        {
            var document = await
                Collection.Find(x => x.ApiKeys.Contains(apiKey))
                    .FirstOrDefaultAsync(ct);

            return (document?.ToApp(), document?.Etag);
        }
    }

    public async Task<(App? App, string? Etag)> GetAsync(string id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbAppRepository/GetAsync"))
        {
            var document = await GetDocumentAsync(id, ct);

            return (document?.ToApp(), document?.Etag);
        }
    }

    public async Task UpsertAsync(App app, string? oldEtag = null,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbAppRepository/UpsertAsync"))
        {
            var document = MongoDbApp.FromApp(app);

            await UpsertDocumentAsync(document.DocId, document, oldEtag, ct);
        }
    }

    public async Task BatchWriteAsync(List<(string Key, CounterMap Counters)> counters,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoDbAppRepository/BatchWriteAsync"))
        {
            var writes = new List<WriteModel<MongoDbApp>>(counters.Count);

            foreach (var (id, values) in counters)
            {
                if (values.Any())
                {
                    var updates = new List<UpdateDefinition<MongoDbApp>>(values.Count);

                    foreach (var (key, value) in values)
                    {
                        var field = $"d.Counters.{key}";

                        updates.Add(Update.Inc(field, value));
                    }

                    var model = new UpdateOneModel<MongoDbApp>(Filter.Eq(x => x.DocId, id), Update.Combine(updates));

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
