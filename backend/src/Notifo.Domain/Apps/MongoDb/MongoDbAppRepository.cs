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
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Counters;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Apps.MongoDb
{
    internal class MongoDbAppRepository : MongoDbStore<MongoDbApp>, IAppRepository
    {
        static MongoDbAppRepository()
        {
            BsonClassMap.RegisterClassMap<App>(map =>
            {
                map.AutoMap();

                map.MapProperty(x => x.Counters)
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

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoDbApp> collection, CancellationToken ct)
        {
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoDbApp>(
                    IndexKeys
                        .Ascending(x => x.Doc.EmailVerificationStatus)),
                null, ct);

            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoDbApp>(
                    IndexKeys
                        .Ascending(x => x.Doc.EmailAddress)),
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

        public async Task<List<App>> QueryNonConfirmedEmailAddressesAsync(CancellationToken ct)
        {
            var documents =
                await Collection.Find(x =>
                        x.Doc.EmailVerificationStatus == EmailVerificationStatus.Failed ||
                        x.Doc.EmailVerificationStatus == EmailVerificationStatus.Pending)
                    .ToListAsync(ct);

            return documents.Select(x => x.ToApp()).ToList();
        }

        public async Task<List<App>> QueryAsync(string contributorId, CancellationToken ct)
        {
            var documents =
                await Collection.Find(x => x.ContributorIds.Contains(contributorId))
                    .ToListAsync(ct);

            return documents.Select(x => x.ToApp()).ToList();
        }

        public async Task<(App? App, string? Etag)> GetByApiKeyAsync(string apiKey, CancellationToken ct)
        {
            var document = await
                Collection.Find(x => x.ApiKeys.Contains(apiKey))
                    .FirstOrDefaultAsync(ct);

            return (document?.ToApp(), document?.Etag);
        }

        public async Task<(App? App, string? Etag)> GetByEmailAddressAsync(string emailAddress, CancellationToken ct)
        {
            var document = await
                Collection.Find(x => x.Doc.EmailAddress == emailAddress)
                    .FirstOrDefaultAsync(ct);

            return (document?.ToApp(), document?.Etag);
        }

        public async Task<(App? App, string? Etag)> GetAsync(string id, CancellationToken ct)
        {
            var document = await GetDocumentAsync(id, ct);

            return (document?.ToApp(), document?.Etag);
        }

        public Task UpsertAsync(App app, string? oldEtag, CancellationToken ct)
        {
            var document = MongoDbApp.FromApp(app);

            return UpsertDocumentAsync(document.DocId, document, oldEtag, ct);
        }

        public Task BatchWriteAsync(List<(string Key, CounterMap Counters)> counters, CancellationToken ct)
        {
            var writes = new List<WriteModel<MongoDbApp>>();

            foreach (var (id, values) in counters)
            {
                if (values.Any())
                {
                    var updates = new List<UpdateDefinition<MongoDbApp>>();

                    foreach (var (key, value) in values)
                    {
                        var field = $"App.Counters.{key}";

                        updates.Add(Update.Inc(field, value));
                    }

                    var model = new UpdateOneModel<MongoDbApp>(Filter.Eq(x => x.DocId, id), Update.Combine(updates));

                    writes.Add(model);
                }
            }

            return Collection.BulkWriteAsync(writes, cancellationToken: ct);
        }
    }
}
