// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Identity.MongoDb
{
    public sealed class MongoDbPersistedGrantStore : MongoDbRepository<PersistedGrant>, IPersistedGrantStore
    {
        static MongoDbPersistedGrantStore()
        {
            BsonClassMap.RegisterClassMap<PersistedGrant>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.Key));
            });
        }

        public MongoDbPersistedGrantStore(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Identity_PersistedGrants";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<PersistedGrant> collection, CancellationToken ct)
        {
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<PersistedGrant>(
                    IndexKeys
                        .Ascending(x => x.SubjectId)
                        .Ascending(x => x.ClientId)
                        .Ascending(x => x.Type)));
        }

        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            return await Collection.Find(x => x.SubjectId == subjectId).ToListAsync();
        }

        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            return await Collection.Find(CreateFilter(filter)).ToListAsync();
        }

        public Task<PersistedGrant> GetAsync(string key)
        {
            return Collection.Find(x => x.Key == key).FirstOrDefaultAsync();
        }

        public Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            return Collection.DeleteManyAsync(CreateFilter(filter));
        }

        public Task RemoveAsync(string key)
        {
            return Collection.DeleteManyAsync(x => x.Key == key);
        }

        public Task StoreAsync(PersistedGrant grant)
        {
            return Collection.ReplaceOneAsync(x => x.Key == grant.Key, grant, UpsertReplace);
        }

        private static FilterDefinition<PersistedGrant> CreateFilter(PersistedGrantFilter filter)
        {
            var fb = Builders<PersistedGrant>.Filter;

            var filters = new List<FilterDefinition<PersistedGrant>>();

            if (!string.IsNullOrWhiteSpace(filter.ClientId))
            {
                filters.Add(fb.Eq(x => x.ClientId, filter.ClientId));
            }

            if (!string.IsNullOrWhiteSpace(filter.SessionId))
            {
                filters.Add(fb.Eq(x => x.SessionId, filter.SessionId));
            }

            if (!string.IsNullOrWhiteSpace(filter.SubjectId))
            {
                filters.Add(fb.Eq(x => x.SubjectId, filter.SubjectId));
            }

            if (!string.IsNullOrWhiteSpace(filter.Type))
            {
                filters.Add(fb.Eq(x => x.Type, filter.Type));
            }

            if (filters.Count > 0)
            {
                return fb.And(filters);
            }

            return new BsonDocument();
        }
    }
}
