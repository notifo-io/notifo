// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using Notifo.Infrastructure;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Subscriptions.MongoDb
{
    public sealed class MongoDbSubscriptionRepository : MongoDbStore<MongoDbSubscription>, ISubscriptionRepository
    {
        public MongoDbSubscriptionRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Subscriptions";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoDbSubscription> collection,
            CancellationToken ct = default)
        {
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoDbSubscription>(
                    IndexKeys
                        .Ascending(x => x.UserId)
                        .Ascending(x => x.AppId)
                        .Ascending("ta.0")
                        .Ascending("ta.1")
                        .Ascending("ta.2")
                        .Ascending("ta.3")
                        .Ascending("ta.4")
                        .Ascending("ta.5")),
                null, ct);

            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoDbSubscription>(
                    IndexKeys
                        .Ascending(x => x.AppId)
                        .Ascending(x => x.TopicPrefix)
                        .Ascending(x => x.UserId),
                    new CreateIndexOptions { Unique = true }),
                null, ct);
        }

        public async Task<IResultList<Subscription>> QueryAsync(string appId, SubscriptionQuery query,
            CancellationToken ct = default)
        {
            using (var activity = Telemetry.Activities.StartActivity("MongoDbSubscriptionRepository/QueryAsync"))
            {
                var filters = new List<FilterDefinition<MongoDbSubscription>>
                {
                    Filter.Eq(x => x.AppId, appId)
                };

                if (!string.IsNullOrWhiteSpace(query.UserId))
                {
                    filters.Add(Filter.Eq(x => x.UserId, query.UserId));
                }

                if (!string.IsNullOrWhiteSpace(query.Query))
                {
                    var regex = new BsonRegularExpression(Regex.Escape(query.Query), "i");

                    filters.Add(Filter.Regex(x => x.TopicPrefix, regex));
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

                return ResultList.Create(resultTotal, resultItems.Select(x => x.ToSubscription()));
            }
        }

        public async IAsyncEnumerable<Subscription> QueryAsync(string appId, TopicId topic, string? userId = null,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("MongoDbSubscriptionRepository/QueryAsyncByTopic"))
            {
                var filter = CreatePrefixFilter(appId, userId, topic, false);

                var find = Collection.Find(filter).SortBy(x => x.UserId);

                var lastSubscription = (MongoDbSubscription?)null;

                using (var cursor = await find.ToCursorAsync(ct))
                {
                    while (await cursor.MoveNextAsync(ct) && !ct.IsCancellationRequested)
                    {
                        foreach (var subscription in cursor.Current)
                        {
                            if (topic.Id.StartsWith(subscription.TopicPrefix, StringComparison.OrdinalIgnoreCase))
                            {
                                if (string.Equals(subscription.UserId, lastSubscription?.UserId, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (subscription.TopicPrefix.Length > lastSubscription!.TopicPrefix.Length)
                                    {
                                        lastSubscription = subscription;
                                    }
                                }
                                else
                                {
                                    if (lastSubscription != null)
                                    {
                                        yield return lastSubscription.ToSubscription();
                                    }

                                    lastSubscription = subscription;
                                }
                            }
                        }
                    }
                }

                if (lastSubscription != null)
                {
                    yield return lastSubscription.ToSubscription();
                }
            }
        }

        public async Task<(Subscription? Subscription, string? Etag)> GetAsync(string appId, string userId, TopicId prefix,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("MongoDbSubscriptionRepository/GetAsync"))
            {
                var topicPrefix = prefix.Id;

                var document =
                    await Collection.Find(x => x.AppId == appId && x.UserId == userId && x.TopicPrefix == topicPrefix)
                        .FirstOrDefaultAsync(ct);

                return (document?.ToSubscription(), document?.Etag);
            }
        }

        public async Task UpsertAsync(Subscription subscription, string? oldEtag = null,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("MongoDbSubscriptionRepository/UpsertAsync"))
            {
                var document = MongoDbSubscription.FromSubscription(subscription);

                await UpsertDocumentAsync(document.DocId, document, oldEtag, ct);
            }
        }

        public async Task DeleteAsync(string appId, string userId, TopicId prefix,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("MongoDbSubscriptionRepository/DeleteAsync"))
            {
                var id = MongoDbSubscription.CreateId(appId, userId, prefix);

                await Collection.DeleteOneAsync(x => x.DocId == id, ct);
            }
        }

        public async Task DeletePrefixAsync(string appId, string userId, TopicId prefix,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("MongoDbSubscriptionRepository/DeletePrefixAsync"))
            {
                var filter = CreatePrefixFilter(appId, userId, prefix, true);

                await Collection.DeleteManyAsync(filter, ct);
            }
        }

        private static FilterDefinition<MongoDbSubscription> CreatePrefixFilter(string appId, string? userId, TopicId topic, bool withUser)
        {
            var filters = new List<FilterDefinition<MongoDbSubscription>>();

            if (withUser)
            {
                filters.Add(Filter.Eq(x => x.UserId, userId));
            }
            else
            {
                filters.Add(Filter.Ne(x => x.UserId, userId));
            }

            filters.Add(Filter.Eq(x => x.AppId, appId));

            var index = 0;

            foreach (var part in topic.GetParts())
            {
                var fieldName = $"ta.{index}";

                filters.Add(
                    Filter.Or(
                        Filter.Eq(fieldName, part),
                        Filter.Exists(fieldName, false)));

                index++;
            }

            return Filter.And(filters);
        }
    }
}
