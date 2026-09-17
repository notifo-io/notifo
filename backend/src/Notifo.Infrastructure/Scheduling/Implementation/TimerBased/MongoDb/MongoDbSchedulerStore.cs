// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NodaTime;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Infrastructure.Scheduling.Implementation.TimerBased.MongoDb;

public sealed class MongoDbSchedulerStore<T>(IMongoDatabase database, SchedulerOptions options) : MongoDbRepository<SchedulerBatch<T>>(database), ISchedulerStore<T>
{
    static MongoDbSchedulerStore()
    {
        BsonClassMap.RegisterClassMap<SchedulerBatch<T>>(cm =>
        {
            cm.AutoMap();

            cm.MapProperty(x => x.GroupKey)
                .SetElementName("Key");
        });
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
                    .Ascending(x => x.GroupKey)
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
            await Collection.UpdateOneAsync(x => x.GroupKey == groupKey && !x.Progressing && x.DueTime <= dueTime,
                Update
                    .SetOnInsert(x => x.Id, Guid.NewGuid().ToString())
                    // The batch must not be handled before the delay of the last job has been elapsed.
                    .Max(x => x.DueTime, dueTime)
                    .SetOnInsert(x => x.GroupKey, groupKey)
                    .SetOnInsert(x => x.Progressing, false)
                    .SetOnInsert(x => x.ProgressingStarted, null)
                    .SetOnInsert(x => x.RetryCount, retryCount)
                    .Set(JobField(key), job),
                Upsert, ct);
        }
    }

    public async Task EnqueueAsync(string key, T job, Instant dueTime, int retryCount = 0,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbSchedulerStore/EnqueueScheduledAsync"))
        {
            await Collection.UpdateOneAsync(x => x.GroupKey == key && !x.Progressing,
                Update
                    .SetOnInsert(x => x.Id, Guid.NewGuid().ToString())
                    // A job can be scheduled again with an earlier due time, e.g. for updates.
                    .Min(x => x.DueTime, dueTime)
                    .SetOnInsert(x => x.GroupKey, key)
                    .SetOnInsert(x => x.Progressing, false)
                    .SetOnInsert(x => x.ProgressingStarted, null)
                    .SetOnInsert(x => x.RetryCount, retryCount)
                    .Set(JobField(key), job),
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

    public async Task<bool> CompleteByKeyAsync(string key,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbSchedulerStore/CompleteByKeyAsync"))
        {
            var result = await Collection.DeleteOneAsync(x => x.GroupKey == key, ct);

            return result.DeletedCount == 1;
        }
    }

    public async Task<bool> CompleteByKeyAsync(string key, string groupKey,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbSchedulerStore/CompleteByKeyAsync"))
        {
            var jobField = JobField(key);

            // Multiple batches can have the same group key, therefore we have to find the batch that contains the job.
            var result =
                await Collection.FindOneAndUpdateAsync(
                    Filter.And(
                        Filter.Eq(x => x.GroupKey, groupKey),
                        Filter.Eq(x => x.Progressing, false),
                        Filter.Exists(jobField)),
                    Update.Unset(jobField),
                    cancellationToken: ct);

            if (result == null)
            {
                return false;
            }

            // Only delete the batch if it is still empty, because other jobs could have been added in the meantime.
            await Collection.DeleteOneAsync(
                Filter.And(
                    Filter.Eq(x => x.Id, result.Id),
                    Filter.Eq(x => x.Progressing, false),
                    Filter.Eq(x => x.JobsV2, new Dictionary<string, T>()),
                    Filter.Eq("Jobs", BsonNull.Value)),
                ct);

            return true;
        }
    }

    private static string JobField(string key)
    {
        // A dot in the key would be interpreted as a path separator and would nest the job in sub documents.
        return "JobsV2." + key.Replace('.', '_');
    }
}
