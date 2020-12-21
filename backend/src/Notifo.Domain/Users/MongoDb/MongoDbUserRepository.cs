// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

namespace Notifo.Domain.Users.MongoDb
{
    public sealed class MongoDbUserRepository : MongoDbStore<MongoDbUser>, IUserRepository
    {
        static MongoDbUserRepository()
        {
            BsonClassMap.RegisterClassMap<User>(map =>
            {
                map.AutoMap();
            });
        }

        public MongoDbUserRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Users";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoDbUser> collection, CancellationToken ct)
        {
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoDbUser>(
                    IndexKeys
                        .Ascending(x => x.Doc.AppId)
                        .Ascending(x => x.Doc.FullName)
                        .Ascending(x => x.Doc.EmailAddress),
                    null),
                null, ct);

            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoDbUser>(
                    IndexKeys
                        .Ascending(x => x.Doc.ApiKey),
                    new CreateIndexOptions { Unique = true }),
                null, ct);
        }

        public async Task<IResultList<User>> QueryAsync(string appId, UserQuery query, CancellationToken ct)
        {
            var filters = new List<FilterDefinition<MongoDbUser>>
            {
                Filter.Eq(x => x.Doc.AppId, appId)
            };

            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                var regex = new BsonRegularExpression(query.Query, "i");

                filters.Add(
                    Filter.Or(
                        Filter.Regex(x => x.Doc.Id, regex),
                        Filter.Regex(x => x.Doc.FullName, regex),
                        Filter.Regex(x => x.Doc.EmailAddress, regex)));
            }

            var filter = Filter.And(filters);

            var taskForItems = Collection.Find(filter).ToListAsync(query, ct);
            var taskForCount = Collection.Find(filter).CountDocumentsAsync(ct);

            await Task.WhenAll(
                taskForItems,
                taskForCount);

            return ResultList.Create(taskForCount.Result, taskForItems.Result.Select(x => x.ToUser()));
        }

        public async Task<(User? User, string? Etag)> GetByApiKeyAsync(string apiKey, CancellationToken ct)
        {
            var document = await Collection.Find(x => x.Doc.ApiKey == apiKey).FirstOrDefaultAsync(ct);

            return (document?.ToUser(), document?.Etag);
        }

        public async Task<(User? User, string? Etag)> GetAsync(string appId, string id, CancellationToken ct)
        {
            var docId = MongoDbUser.CreateId(appId, id);

            var document = await GetDocumentAsync(docId, ct);

            return (document?.ToUser(), document?.Etag);
        }

        public Task UpsertAsync(User user, string? oldEtag, CancellationToken ct)
        {
            var docId = MongoDbUser.FromUser(user);

            return UpsertDocumentAsync(docId.DocId, docId, oldEtag, ct);
        }

        public Task DeleteAsync(string appId, string id, CancellationToken ct)
        {
            var docId = MongoDbUser.CreateId(appId, id);

            return Collection.DeleteOneAsync(x => x.DocId == docId, ct);
        }

        public Task StoreAsync(List<((string AppId, string UserId) Key, CounterMap Counters)> counters, CancellationToken ct)
        {
            var writes = new List<WriteModel<MongoDbUser>>();

            foreach (var ((appId, id), values) in counters)
            {
                if (values.Any())
                {
                    var docId = MongoDbUser.CreateId(appId, id);

                    var updates = new List<UpdateDefinition<MongoDbUser>>();

                    foreach (var (key, value) in values)
                    {
                        updates.Add(Update.Inc($"d.Counters.{key}", value));
                    }

                    var model = new UpdateOneModel<MongoDbUser>(Filter.Eq(x => x.DocId, docId), Update.Combine(updates));

                    writes.Add(model);
                }
            }

            return Collection.BulkWriteAsync(writes, cancellationToken: ct);
        }
    }
}
