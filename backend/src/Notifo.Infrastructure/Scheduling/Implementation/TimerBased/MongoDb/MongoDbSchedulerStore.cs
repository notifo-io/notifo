// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using NodaTime;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Infrastructure.Scheduling.Implementation.TimerBased.MongoDb;

public sealed class MongoDbSchedulerStore<T> : MongoDbRepository<SchedulerBatch<T>>, ISchedulerStore<T>
{
    private readonly SchedulerOptions options;

    public MongoDbSchedulerStore(IMongoDatabase database, SchedulerOptions options)
        : base(database)
    {
        this.options = options;
    }

    protected override string CollectionName()
    {
        return $"Scheduler_{options.QueueName.ToLowerInvariant()}";
    }

    protected override async Task SetupCollectionAsync(IMongoCollection<SchedulerBatch<T>> collection,
        CancellationToken ct = default)
    {
        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<SchedulerBatch<T>>(
                IndexKeys
                    .Ascending(x => x.Key)
                    .Ascending(x => x.Progressing)
                    .Ascending(x => x.DueTime)),
            null, ct);

        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<SchedulerBatch<T>>(
                IndexKeys
                    .Ascending(x => x.Progressing)),
            null, ct);
    }

    public async Task<SchedulerBatch<T>?> DequeueAsync(Instant time,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbSchedulerStore/DequeueAsync"))
        {
            return await Collection.FindOneAndUpdateAsync(x => !x.Progressing && x.DueTime <= time,
                Update
                    .Set(x => x.Progressing, true)
                    .Set(x => x.ProgressingStarted, time),
                cancellationToken: ct);
        }
    }

    public async Task ResetDeadAsync(Instant oldTime, Instant next,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbSchedulerStore/ResetDeadAsync"))
        {
            await Collection.UpdateManyAsync(x => x.Progressing && x.ProgressingStarted < oldTime,
                Update
                    .Set(x => x.DueTime, next)
                    .Set(x => x.Progressing, false)
                    .Set(x => x.ProgressingStarted, null)
                    .Inc(x => x.RetryCount, 1),
                cancellationToken: ct);
        }
    }

    public async Task RetryAsync(string id, Instant next,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbSchedulerStore/RetryAsync"))
        {
            await Collection.UpdateOneAsync(x => x.Id == id,
                Update
                    .Set(x => x.DueTime, next)
                    .Set(x => x.Progressing, false)
                    .Set(x => x.ProgressingStarted, null)
                    .Inc(x => x.RetryCount, 1),
                cancellationToken: ct);
        }
    }

    public async Task EnqueueGroupedAsync(string key, string groupKey, T job, Instant dueTime, int retryCount = 0,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbSchedulerStore/EnqueueGroupedAsync"))
        {
            await Collection.UpdateOneAsync(x => x.Key == groupKey && !x.Progressing && x.DueTime <= dueTime,
                Update
                    .SetOnInsert(x => x.DueTime, dueTime)
                    .SetOnInsert(x => x.Id, Guid.NewGuid().ToString())
                    .SetOnInsert(x => x.Key, groupKey)
                    .SetOnInsert(x => x.Progressing, false)
                    .SetOnInsert(x => x.ProgressingStarted, null)
                    .SetOnInsert(x => x.RetryCount, retryCount)
                    .Set($"JobsV2.{key}", job),
                Upsert, ct);
        }
    }

    public async Task EnqueueAsync(string key, T job, Instant dueTime, int retryCount = 0,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbSchedulerStore/EnqueueScheduledAsync"))
        {
            await Collection.UpdateOneAsync(x => x.Key == key && !x.Progressing,
                Update
                    .SetOnInsert(x => x.DueTime, dueTime)
                    .SetOnInsert(x => x.Id, Guid.NewGuid().ToString())
                    .SetOnInsert(x => x.Key, key)
                    .SetOnInsert(x => x.Progressing, false)
                    .SetOnInsert(x => x.ProgressingStarted, null)
                    .SetOnInsert(x => x.RetryCount, retryCount)
                    .Set($"JobsV2.{key}", job),
                Upsert, ct);
        }
    }

    public async Task CompleteAsync(string id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbSchedulerStore/CompleteAsync"))
        {
            await Collection.DeleteOneAsync(x => x.Id == id, ct);
        }
    }

    public async Task CompleteByKeyAsync(string key,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbSchedulerStore/CompleteByKeyAsync"))
        {
            await Collection.DeleteOneAsync(x => x.Key == key, ct);
        }
    }
}
