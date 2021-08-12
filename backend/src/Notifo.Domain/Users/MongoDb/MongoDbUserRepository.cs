﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
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
            BsonClassMap.RegisterClassMap<User>(cm =>
            {
                cm.AutoMap();

                cm.SetIgnoreExtraElements(true);
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

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoDbUser> collection,
            CancellationToken ct)
        {
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoDbUser>(
                    IndexKeys
                        .Ascending(x => x.Doc.AppId)
                        .Ascending(x => x.Doc.FullName)
                        .Ascending(x => x.Doc.EmailAddress)),
                null, ct);

            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoDbUser>(
                    IndexKeys
                        .Ascending(x => x.Doc.ApiKey),
                    new CreateIndexOptions { Unique = true }),
                null, ct);
        }

        public async IAsyncEnumerable<string> QueryIdsAsync(string appId, [EnumeratorCancellation] CancellationToken ct)
        {
            using (Telemetry.Activities.StartMethod<MongoDbUserRepository>())
            {
                var find = Collection.Find(x => x.Doc.AppId == appId).Only(x => x.Doc.Id);

                using (var cursor = await find.ToCursorAsync(ct))
                {
                    while (await cursor.MoveNextAsync(ct) && !ct.IsCancellationRequested)
                    {
                        foreach (var user in cursor.Current)
                        {
                            yield return user["d"]["_id"].AsString;
                        }
                    }
                }
            }
        }

        public async Task<IResultList<User>> QueryAsync(string appId, UserQuery query,
            CancellationToken ct)
        {
            using (var activity = Telemetry.Activities.StartMethod<MongoDbUserRepository>())
            {
                var filters = new List<FilterDefinition<MongoDbUser>>
                {
                    Filter.Eq(x => x.Doc.AppId, appId)
                };

                if (!string.IsNullOrWhiteSpace(query.Query))
                {
                    var regex = new BsonRegularExpression(Regex.Escape(query.Query), "i");

                    filters.Add(
                        Filter.Or(
                            Filter.Regex(x => x.Doc.Id, regex),
                            Filter.Regex(x => x.Doc.FullName, regex),
                            Filter.Regex(x => x.Doc.EmailAddress, regex)));
                }

                var filter = Filter.And(filters);

                var resultItems = await Collection.Find(filter).ToListAsync(query, ct);
                var resultTotal = (long)resultItems.Count;

                if (query.ShouldQueryTotal(resultItems))
                {
                    resultTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
                }

                activity?.SetTag("numResults", resultItems.Count);
                activity?.SetTag("numTotal", resultTotal);

                return ResultList.Create(resultTotal, resultItems.Select(x => x.ToUser()));
            }
        }

        public async Task<(User? User, string? Etag)> GetByApiKeyAsync(string apiKey,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartMethod<MongoDbUserRepository>())
            {
                var document = await Collection.Find(x => x.Doc.ApiKey == apiKey).FirstOrDefaultAsync(ct);

                return (document?.ToUser(), document?.Etag);
            }
        }

        public async Task<(User? User, string? Etag)> GetAsync(string appId, string id,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartMethod<MongoDbUserRepository>())
            {
                var docId = MongoDbUser.CreateId(appId, id);

                var document = await GetDocumentAsync(docId, ct);

                return (document?.ToUser(), document?.Etag);
            }
        }

        public Task UpsertAsync(User user, string? oldEtag,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartMethod<MongoDbUserRepository>())
            {
                var docId = MongoDbUser.FromUser(user);

                return UpsertDocumentAsync(docId.DocId, docId, oldEtag, ct);
            }
        }

        public Task DeleteAsync(string appId, string id,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartMethod<MongoDbUserRepository>())
            {
                var docId = MongoDbUser.CreateId(appId, id);

                return Collection.DeleteOneAsync(x => x.DocId == docId, ct);
            }
        }

        public Task BatchWriteAsync(List<((string AppId, string UserId) Key, CounterMap Counters)> counters,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartMethod<MongoDbUserRepository>())
            {
                var writes = new List<WriteModel<MongoDbUser>>(counters.Count);

                foreach (var ((appId, id), values) in counters)
                {
                    if (values.Any())
                    {
                        var docId = MongoDbUser.CreateId(appId, id);

                        var updates = new List<UpdateDefinition<MongoDbUser>>(values.Count);

                        foreach (var (key, value) in values)
                        {
                            updates.Add(Update.Inc($"d.Counters.{key}", value));
                        }

                        var model = new UpdateOneModel<MongoDbUser>(Filter.Eq(x => x.DocId, docId), Update.Combine(updates))
                        {
                            IsUpsert = true
                        };

                        writes.Add(model);
                    }
                }

                return Collection.BulkWriteAsync(writes, cancellationToken: ct);
            }
        }
    }
}
