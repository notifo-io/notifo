// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Infrastructure.KeyValueStore.MongoDb
{
    public sealed class MongoDbKeyValueStore : MongoDbRepository<MongoDbKeyValue>, IKeyValueStore
    {
        public MongoDbKeyValueStore(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "KeyValueStore";
        }

        public async Task<string?> GetAsync(string key,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(key);

            var result = await Collection.Find(ByKey(key)).FirstOrDefaultAsync(ct);

            return result?.Value;
        }

        public async Task<bool> RemvoveAsync(string key,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(key);

            var result = await Collection.DeleteOneAsync(ByKey(key), ct);

            return result.DeletedCount == 1;
        }

        public async Task<bool> SetAsync(string key, string? value,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(key);

            var result = await Collection.UpdateOneAsync(ByKey(key), Update.Set(x => x.Value, value), Upsert, ct);

            return result.IsModifiedCountAvailable && result.ModifiedCount == 1;
        }

        public async Task<string?> SetIfNotExistsAsync(string key, string? value,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(key);

            try
            {
                await Collection.InsertOneAsync(new MongoDbKeyValue { Key = key, Value = value }, null, ct);

                return value;
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
                {
                    return await GetAsync(key, ct);
                }

                throw;
            }
        }

        private static FilterDefinition<MongoDbKeyValue> ByKey(string key)
        {
            return Filter.Eq(x => x.Key, key);
        }
    }
}
