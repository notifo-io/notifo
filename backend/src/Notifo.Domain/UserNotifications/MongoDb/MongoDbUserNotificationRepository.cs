// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using NodaTime;
using Notifo.Infrastructure;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.UserNotifications.MongoDb;

public sealed class MongoDbUserNotificationRepository : MongoDbRepository<UserNotification>, IUserNotificationRepository
{
    private readonly UserNotificationsOptions options;
    private readonly ILogger<MongoDbUserNotificationRepository> log;

    static MongoDbUserNotificationRepository()
    {
        BsonClassMap.RegisterClassMap<BaseUserNotification>(cm =>
        {
            cm.AutoMap();

            cm.MapIdProperty(x => x.Id)
                .SetSerializer(new GuidSerializer(BsonType.String));
        });

        BsonClassMap.RegisterClassMap<ChannelSendInfo>(cm =>
        {
            cm.AutoMap();

            cm.MapProperty(x => x.FirstConfirmed)
                .SetIgnoreIfNull(true);

            cm.MapProperty(x => x.FirstDelivered)
                .SetIgnoreIfNull(true);

            cm.MapProperty(x => x.FirstSeen)
                .SetIgnoreIfNull(true);
        });

        BsonClassMap.RegisterClassMap<UserNotification>(cm =>
        {
            cm.AutoMap();

            cm.MapProperty(x => x.FirstConfirmed)
                .SetIgnoreIfNull(true);

            cm.MapProperty(x => x.FirstDelivered)
                .SetIgnoreIfNull(true);

            cm.MapProperty(x => x.FirstSeen)
                .SetIgnoreIfNull(true);
        });

        BsonClassMap.RegisterClassMap<UserNotificationChannel>(cm =>
        {
            cm.AutoMap();

            cm.MapProperty(x => x.FirstConfirmed)
                .SetIgnoreIfNull(true);

            cm.MapProperty(x => x.FirstDelivered)
                .SetIgnoreIfNull(true);

            cm.MapProperty(x => x.FirstSeen)
                .SetIgnoreIfNull(true);

            cm.MapProperty(x => x.Status)
                .SetSerializer(new StatusDictionarySerializer());
        });
    }

    public MongoDbUserNotificationRepository(IMongoDatabase database, IOptions<UserNotificationsOptions> options,
        ILogger<MongoDbUserNotificationRepository> log)
        : base(database)
    {
        this.options = options.Value;

        this.log = log;
    }

    protected override string CollectionName()
    {
        return "Notifications";
    }

    protected override async Task SetupCollectionAsync(IMongoCollection<UserNotification> collection,
        CancellationToken ct = default)
    {
        await Collection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserNotification>(
                IndexKeys
                    .Ascending(x => x.AppId)
                    .Ascending(x => x.UserId)
                    .Ascending(x => x.Updated)
                    .Ascending(x => x.IsDeleted)
                    .Descending(x => x.Created)),
            null, ct);

        await Collection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserNotification>(
                IndexKeys
                    .Ascending(x => x.AppId)
                    .Ascending(x => x.CorrelationId)
                    .Ascending(x => x.Updated)
                    .Ascending(x => x.IsDeleted)
                    .Descending(x => x.Created)),
            null, ct);

        await Collection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserNotification>(
                IndexKeys
                    .Descending(x => x.Created),
                new CreateIndexOptions
                {
                    ExpireAfter = options.RetentionTime
                }),
            null, ct);
    }

    public async Task<bool> IsHandledOrConfirmedAsync(Guid id, string channel, Guid configurationId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbUserNotificationRepository/IsHandledOrConfirmedAsync"))
        {
            var filter =
                Filter.And(
                    Filter.Eq(x => x.Id, id),
                    Filter.Or(
                        Filter.Exists(x => x.FirstConfirmed),
                        Filter.Eq($"Channels.{channel}.Status.{configurationId}.Status", ProcessStatus.Handled)));

            var count =
                await Collection.Find(filter).Limit(1)
                    .CountDocumentsAsync(ct);

            return count == 1;
        }
    }

    public async Task<bool> IsHandledOrSeenAsync(Guid id, string channel, Guid configurationId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbUserNotificationRepository/IsHandledOrSeenAsync"))
        {
            var filter =
                Filter.And(
                    Filter.Eq(x => x.Id, id),
                    Filter.Or(
                        Filter.Exists(x => x.FirstSeen),
                        Filter.Eq($"Channels.{channel}.Status.{configurationId}.Status", ProcessStatus.Handled)));

            var count =
                await Collection.Find(filter).Limit(1)
                    .CountDocumentsAsync(ct);

            return count == 1;
        }
    }

    public async Task<bool> IsHandledAsync(Guid id, string channel, Guid configurationId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbUserNotificationRepository/IsHandledAsync"))
        {
            var filter =
                Filter.And(
                    Filter.Eq(x => x.Id, id),
                    Filter.Eq($"Channels.{channel}.Status.{configurationId}.Status", ProcessStatus.Handled));

            var count =
                await Collection.Find(filter).Limit(1)
                    .CountDocumentsAsync(ct);

            return count == 1;
        }
    }

    public async Task<IResultList<UserNotification>> QueryAsync(string appId, string userId, UserNotificationQuery query,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("MongoDbUserNotificationRepository/QueryAsync"))
        {
            var filter = BuildFilter(appId, userId, query);

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

    public async Task<IResultList<UserNotification>> QueryAsync(string appId, UserNotificationQuery query,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("MongoDbUserNotificationRepository/QueryAsync"))
        {
            var filter = BuildFilter(appId, query);

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

    public async Task<IReadOnlyDictionary<string, Instant>> QueryLastNotificationsAsync(string appId, IEnumerable<string> userIds,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("MongoDbUserNotificationRepository/QueryLastNotificationsAsync"))
        {
            var result = new Dictionary<string, Instant>();

            foreach (var userId in userIds)
            {
                var filter = BuildFilter(appId, userId);

                var item = await Collection.Find(filter).Limit(1).SortByDescending(x => x.Created).Only(x => x.Created).FirstOrDefaultAsync(ct);

                if (item != null && item.TryGetElement("Created", out var created))
                {
                    result[userId] = Instant.FromDateTimeUtc(created.Value.ToUniversalTime());
                }
            }

            return result;
        }
    }

    public async Task<UserNotification?> FindAsync(Guid id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbUserNotificationRepository/FindAsync"))
        {
            var entity = await Collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

            return entity;
        }
    }

    public async Task DeleteAsync(Guid id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbUserNotificationRepository/DeleteAsync"))
        {
            await Collection.UpdateOneAsync(x => x.Id == id, Update.Set(x => x.IsDeleted, true), cancellationToken: ct);
        }
    }

    public async Task BatchWriteAsync((TrackingToken Token, ProcessStatus Status, string? Detail)[] updates, Instant now,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbUserNotificationRepository/BatchWriteAsync"))
        {
            var batch = await TrackingBatch.CreateAsync(Collection, updates.Select(x => x.Token), ct);

            batch.UpdateStatus(updates, now);

            await batch.WriteAsync(ct);
        }
    }

    public async Task<IReadOnlyList<(UserNotification, bool Updated)>> TrackConfirmedAsync(TrackingToken[] tokens, Instant now,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbUserNotificationRepository/TrackConfirmedAsync"))
        {
            var batch = await TrackingBatch.CreateAsync(Collection, tokens, ct);

            batch.MarkIfNotConfirmed(tokens, now);
            batch.MarkIfNotSeen(tokens, now);
            batch.MarkIfNotDelivered(tokens, now);

            await batch.WriteAsync(ct);

            return batch.GetNotifications();
        }
    }

    public async Task<IReadOnlyList<(UserNotification, bool Updated)>> TrackSeenAsync(TrackingToken[] tokens, Instant now,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbUserNotificationRepository/TrackSeenAsync"))
        {
            var batch = await TrackingBatch.CreateAsync(Collection, tokens, ct);

            batch.MarkIfNotSeen(tokens, now);
            batch.MarkIfNotDelivered(tokens, now);

            await batch.WriteAsync(ct);

            return batch.GetNotifications();
        }
    }

    public async Task<IReadOnlyList<(UserNotification, bool Updated)>> TrackDeliveredAsync(TrackingToken[] tokens, Instant now,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbUserNotificationRepository/TrackDeliveredAsync"))
        {
            var batch = await TrackingBatch.CreateAsync(Collection, tokens, ct);

            batch.MarkIfNotDelivered(tokens, now);

            await batch.WriteAsync(ct);

            return batch.GetNotifications();
        }
    }

    public async Task InsertAsync(UserNotification notification,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbUserNotificationRepository/InsertAsync"))
        {
            try
            {
                await Collection.InsertOneAsync(notification, null, ct);

                await CleanupAsync(notification, ct);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                throw new UniqueConstraintException();
            }
        }
    }

    private async Task CleanupAsync(UserNotification notification,
        CancellationToken ct)
    {
        if (options.MaxItemsPerUser is <= 0 or >= int.MaxValue)
        {
            return;
        }

        using (Telemetry.Activities.StartActivity("MongoDbUserNotificationRepository/CleanupAsync"))
        {
            try
            {
                var filter = BuildFilter(notification);

                // Sort descending because we do not want to delete the newest elements.
                var oldNotifications = Collection.Find(filter).Skip(options.MaxItemsPerUser).SortByDescending(x => x.Created).Only(x => x.Id).ToAsyncEnumerable(ct);

                await foreach (var batch in oldNotifications.Chunk(5000, ct))
                {
                    var ids = batch.Select(x => x["_id"].AsString!);

                    await Collection.DeleteManyAsync(Filter.In("_id", ids), ct);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to cleanup notifications.");
            }
        }
    }

    private static FilterDefinition<UserNotification> BuildFilter(UserNotification notification)
    {
        var filters = new List<FilterDefinition<UserNotification>>
        {
            Filter.Eq(x => x.AppId, notification.AppId),
            Filter.Eq(x => x.UserId, notification.UserId)
        };

        return Filter.And(filters);
    }

    private static FilterDefinition<UserNotification> BuildFilter(string appId, string userId, UserNotificationQuery? query = null)
    {
        var filters = new List<FilterDefinition<UserNotification>>
        {
            Filter.Eq(x => x.AppId, appId),
            Filter.Eq(x => x.UserId, userId)
        };

        AddDefaultFilters(query, filters);

        if (!string.IsNullOrWhiteSpace(query?.CorrelationId))
        {
            filters.Add(Filter.Eq(x => x.CorrelationId, query.CorrelationId));
        }

        return Filter.And(filters);
    }

    private static FilterDefinition<UserNotification> BuildFilter(string appId, UserNotificationQuery? query = null)
    {
        var filters = new List<FilterDefinition<UserNotification>>
        {
            Filter.Eq(x => x.AppId, appId)
        };

        if (!string.IsNullOrWhiteSpace(query?.CorrelationId))
        {
            filters.Add(Filter.Eq(x => x.CorrelationId, query.CorrelationId));
        }
        else
        {
            filters.Add(Filter.Gte(x => x.CorrelationId, null));
        }

        AddDefaultFilters(query, filters);

        return Filter.And(filters);
    }

    private static void AddDefaultFilters(UserNotificationQuery? query, List<FilterDefinition<UserNotification>> filters)
    {
        // Always query by updated flag to force the index to be used.
        filters.Add(Filter.Gte(x => x.Updated, query?.After ?? default));

        switch (query?.Scope)
        {
            case UserNotificationQueryScope.Deleted:
                filters.Add(Filter.Eq(x => x.IsDeleted, true));
                break;
            case UserNotificationQueryScope.NonDeleted:
                filters.Add(Filter.Ne(x => x.IsDeleted, true));
                break;
            default:
                filters.Add(Filter.RawNe(nameof(UserNotification.IsDeleted), BsonValue.Create("invalid")));
                break;
        }

        if (!string.IsNullOrWhiteSpace(query?.Query))
        {
            var regex = new BsonRegularExpression(Regex.Escape(query.Query), "i");

            filters.Add(Filter.Regex(x => x.Formatting.Subject, regex));
        }

        if (query?.Channels?.Length > 0)
        {
            var channelFilters = query.Channels.Map(x => Filter.Eq($"Channels.{x}.Setting.Send", "Send"));

            filters.Add(Filter.Or(channelFilters));
        }
    }
}
