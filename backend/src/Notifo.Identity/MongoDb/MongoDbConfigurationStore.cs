// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Notifo.Identity.Dynamic;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Identity.MongoDb;

public sealed class MongoDbConfigurationStore<T> : MongoDbRepository<MongoDbConfiguration<T>>, IConfigurationStore<T> where T : class
{
    public MongoDbConfigurationStore(IMongoDatabase database)
        : base(database)
    {
    }

    protected override string CollectionName()
    {
        return "Identity_Configuration";
    }

    protected override Task SetupCollectionAsync(IMongoCollection<MongoDbConfiguration<T>> collection,
        CancellationToken ct = default)
    {
        return collection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoDbConfiguration<T>>(
                IndexKeys.Ascending(x => x.Expires),
                new CreateIndexOptions
                {
                    ExpireAfter = TimeSpan.Zero
                }),
            cancellationToken: ct);
    }

    public async Task<T?> GetAsync(string key,
        CancellationToken ct = default)
    {
        var entity = await Collection.Find(x => x.Id == key).FirstOrDefaultAsync(ct);

        return entity?.Value;
    }

    public async Task SetAsync(string key, T value, TimeSpan ttl,
        CancellationToken ct = default)
    {
        var expires = DateTime.UtcNow + ttl;

        await Collection.UpdateOneAsync(
            Filter.Eq(x => x.Id, key),
            Update
                .SetOnInsert(x => x.Id, key)
                .Set(x => x.Value, value)
                .Set(x => x.Expires, expires),
            Upsert,
            cancellationToken: ct);
    }
}
