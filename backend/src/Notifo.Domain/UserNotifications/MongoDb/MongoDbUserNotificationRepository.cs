// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Notifo.Infrastructure;
using Notifo.Infrastructure.MongoDb;
using Squidex.Hosting;

namespace Notifo.Domain.UserNotifications.MongoDb
{
    public sealed class MongoDbUserNotificationRepository : MongoDbRepository<UserNotification>, IUserNotificationRepository, IInitializable
    {
        static MongoDbUserNotificationRepository()
        {
            BsonClassMap.RegisterClassMap<UserNotification>(cm =>
            {
                cm.AutoMap();

                cm.SetIgnoreExtraElements(true);

                cm.MapProperty(x => x.FirstSeen)
                    .SetIgnoreIfNull(true);

                cm.MapProperty(x => x.FirstConfirmed)
                    .SetIgnoreIfNull(true);
            });

            BsonClassMap.RegisterClassMap<BaseUserNotification>(cm =>
            {
                cm.AutoMap();

                cm.SetIgnoreExtraElements(true);

                cm.MapIdProperty(x => x.Id)
                    .SetSerializer(new GuidSerializer(BsonType.String));
            });

            BsonClassMap.RegisterClassMap<UserNotificationChannel>(cm =>
            {
                cm.AutoMap();

                cm.SetIgnoreExtraElements(true);

                cm.MapProperty(x => x.Status)
                    .SetSerializer(new DictionaryInterfaceImplementerSerializer<Dictionary<string, ChannelSendInfo>, string, ChannelSendInfo>()
                        .WithKeySerializer(new Base64Serializer()));
            });
        }

        public MongoDbUserNotificationRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Notifications";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<UserNotification> collection,
            CancellationToken ct)
        {
            await Collection.Indexes.CreateOneAsync(
                new CreateIndexModel<UserNotification>(
                    IndexKeys
                        .Ascending(x => x.AppId)
                        .Ascending(x => x.UserId)
                        .Ascending(x => x.Updated)
                        .Ascending(x => x.IsDeleted)),
                null, ct);
        }

        public async Task<bool> IsConfirmedOrHandledAsync(Guid id, string channel, string configuration,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartMethod<MongoDbUserNotificationRepository>())
            {
                var filter =
                   Filter.And(
                       Filter.Eq(x => x.Id, id),
                       Filter.Or(
                            Filter.Exists(x => x.FirstConfirmed),
                            Filter.Eq($"Channels.{channel}.Status.{configuration}.Status", ProcessStatus.Handled)));

                var count =
                    await Collection.Find(filter).Limit(1)
                        .CountDocumentsAsync(ct);

                return count == 1;
            }
        }

        public async Task<IResultList<UserNotification>> QueryAsync(string appId, string userId, UserNotificationQuery query,
            CancellationToken ct)
        {
            using (var activity = Telemetry.Activities.StartMethod<MongoDbUserNotificationRepository>())
            {
                var filters = new List<FilterDefinition<UserNotification>>
                {
                    Filter.Eq(x => x.AppId, appId),
                    Filter.Eq(x => x.UserId, userId),
                    Filter.Gte(x => x.Updated, query.After)
                };

                switch (query.Scope)
                {
                    case UserNotificationQueryScope.Deleted:
                        {
                            filters.Add(Filter.Eq(x => x.IsDeleted, true));
                            break;
                        }

                    case UserNotificationQueryScope.NonDeleted:
                        {
                            filters.Add(
                                Filter.Or(
                                    Filter.Exists(x => x.IsDeleted, false),
                                    Filter.Eq(x => x.IsDeleted, false)));
                            break;
                        }
                }

                if (!string.IsNullOrWhiteSpace(query.Query))
                {
                    var regex = new BsonRegularExpression(Regex.Escape(query.Query), "i");

                    filters.Add(Filter.Regex(x => x.Formatting.Subject, regex));
                }

                var filter = Filter.And(filters);

                var resultItems = await Collection.Find(filter).SortByDescending(x => x.Created).ToListAsync(query, ct);
                var resultTotal = (long)resultItems.Count;

                if (query.ShouldQueryTotal(resultItems))
                {
                    resultTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
                }

                activity?.SetTag("numResults", resultItems.Count);
                activity?.SetTag("numTotal", resultTotal);

                return ResultList.Create(resultTotal, resultItems);
            }
        }

        public async Task<UserNotification?> FindAsync(Guid id,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartMethod<MongoDbUserNotificationRepository>())
            {
                var entity = await Collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

                return entity;
            }
        }

        public async Task DeleteAsync(Guid id,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartMethod<MongoDbUserNotificationRepository>())
            {
                await Collection.UpdateOneAsync(x => x.Id == id, Update.Set(x => x.IsDeleted, true), cancellationToken: ct);
            }
        }

        public async Task TrackSeenAsync(IEnumerable<Guid> ids, HandledInfo handle,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartMethod<MongoDbUserNotificationRepository>())
            {
                var writes = new List<WriteModel<UserNotification>>();

                var timestamp = handle.Timestamp;

                foreach (var id in ids)
                {
                    var updateSeen =
                        Filter.Eq(x => x.Id, id);

                    var updateConfirmed =
                        Filter.And(
                            Filter.Eq(x => x.Id, id),
                            Filter.Eq(x => x.Formatting.ConfirmMode, ConfirmMode.Seen));

                    writes.Add(new UpdateOneModel<UserNotification>(
                        Filter.And(
                            updateSeen,
                            Filter.Exists(x => x.FirstSeen, false)),
                        Update
                            .Set(x => x.FirstSeen, handle).Max(x => x.Updated, timestamp)));

                    writes.Add(new UpdateOneModel<UserNotification>(
                        Filter.And(
                            updateConfirmed,
                            Filter.Exists(x => x.FirstConfirmed, false)),
                        Update
                            .Set(x => x.FirstConfirmed, handle).Max(x => x.Updated, timestamp)));

                    var channel = handle.Channel;

                    if (!string.IsNullOrWhiteSpace(channel))
                    {
                        writes.Add(new UpdateOneModel<UserNotification>(
                            Filter.And(
                                updateSeen,
                                Filter.Exists($"Channels.{channel}")),
                            Update.Max($"$Channels.{channel}.LastSeen", timestamp)));

                        writes.Add(new UpdateOneModel<UserNotification>(
                            Filter.And(
                                updateConfirmed,
                                Filter.Exists($"Channels.{channel}")),
                            Update.Max($"$Channels.{channel}.LastConfirmed", timestamp)));
                    }
                }

                if (writes.Count == 0)
                {
                    return;
                }

                await Collection.BulkWriteAsync(writes, cancellationToken: ct);
            }
        }

        public async Task<UserNotification?> TrackConfirmedAsync(Guid id, HandledInfo handle,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartMethod<MongoDbUserNotificationRepository>())
            {
                await TrackSeenAsync(Enumerable.Repeat(id, 1), handle, ct);

                var timestamp = handle.Timestamp;

                var updateConfirmed =
                    Filter.And(
                        Filter.Eq(x => x.Id, id),
                        Filter.Eq(x => x.Formatting.ConfirmMode, ConfirmMode.Explicit));

                var channel = handle.Channel;

                if (!string.IsNullOrWhiteSpace(channel))
                {
                    await Collection.FindOneAndUpdateAsync(
                        Filter.And(
                            updateConfirmed,
                            Filter.Exists($"Channels.{channel}")),
                        Update.Set($"$Channels.{channel}.LastSeen", timestamp),
                        cancellationToken: ct);
                }

                var entity =
                    await Collection.FindOneAndUpdateAsync(
                        Filter.And(
                            updateConfirmed,
                            Filter.Exists(x => x.FirstConfirmed, false)),
                        Update
                            .Set(x => x.FirstSeen, handle).Max(x => x.Updated, timestamp),
                        cancellationToken: ct);

                if (entity != null)
                {
                    entity.FirstConfirmed = handle;
                    entity.Updated = handle.Timestamp;
                }

                return entity;
            }
        }

        public async Task BatchWriteAsync(IEnumerable<(Guid Id, string Channel, string Configuraton, ChannelSendInfo Info)> updates,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartMethod<MongoDbUserNotificationRepository>())
            {
                var writes = new List<WriteModel<UserNotification>>();

                var documentUpdates = new List<UpdateDefinition<UserNotification>>();

                foreach (var group in updates.GroupBy(x => x.Id))
                {
                    documentUpdates.Clear();

                    foreach (var (_, channel, configuration, info) in group)
                    {
                        var path = $"Channels.{channel}.Status.{configuration.ToBase64()}";

                        documentUpdates.Add(Update.Set($"{path}.Detail", info.Detail));
                        documentUpdates.Add(Update.Set($"{path}.Status", info.Status));
                        documentUpdates.Add(Update.Set($"{path}.LastUpdate", info.LastUpdate));
                    }

                    var update = Update.Combine(documentUpdates);

                    writes.Add(new UpdateOneModel<UserNotification>(Filter.Eq(x => x.Id, group.Key), update));
                }

                if (writes.Count == 0)
                {
                    return;
                }

                await Collection.BulkWriteAsync(writes, cancellationToken: ct);
            }
        }

        public async Task InsertAsync(UserNotification notification,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartMethod<MongoDbUserNotificationRepository>())
            {
                try
                {
                    await Collection.InsertOneAsync(notification, null, ct);
                }
                catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    throw new UniqueConstraintException();
                }
            }
        }
    }
}
