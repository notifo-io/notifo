// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using NodaTime;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Infrastructure.Scheduling.TimerBased.MongoDb
{
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

        protected override async Task SetupCollectionAsync(IMongoCollection<SchedulerBatch<T>> collection, CancellationToken ct)
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

        public async Task<SchedulerBatch<T>?> DequeueAsync(Instant time)
        {
            return await Collection.FindOneAndUpdateAsync(x => !x.Progressing && x.DueTime <= time,
                Update
                    .Set(x => x.Progressing, true)
                    .Set(x => x.ProgressingStarted, time));
        }

        public Task ResetDeadAsync(Instant oldTime, Instant next)
        {
            return Collection.UpdateManyAsync(x => x.Progressing && x.ProgressingStarted < oldTime,
                Update
                    .Set(x => x.DueTime, next)
                    .Set(x => x.Progressing, false)
                    .Set(x => x.ProgressingStarted, null));
        }

        public Task RetryAsync(string id, Instant next)
        {
            return Collection.UpdateOneAsync(x => x.Id == id,
                Update
                    .Set(x => x.DueTime, next)
                    .Set(x => x.Progressing, false)
                    .Set(x => x.ProgressingStarted, null)
                    .Inc(x => x.RetryCount, 1));
        }

        public Task EnqueueGroupedAsync(string key, T job, Instant delay, int retryCount, CancellationToken ct)
        {
            return Collection.UpdateOneAsync(x => x.Key == key && !x.Progressing && x.DueTime <= delay,
                Update
                    .Min(x => x.DueTime, delay)
                    .SetOnInsert(x => x.Id, Guid.NewGuid().ToString())
                    .SetOnInsert(x => x.Key, key)
                    .SetOnInsert(x => x.Progressing, false)
                    .SetOnInsert(x => x.ProgressingStarted, null)
                    .SetOnInsert(x => x.RetryCount, retryCount)
                    .AddToSet(x => x.Jobs, job),
                Upsert, ct);
        }

        public Task EnqueueScheduledAsync(string key, T job, Instant dueTime, int retryCount, CancellationToken ct)
        {
            return Collection.UpdateOneAsync(x => x.Key == key && !x.Progressing,
                Update
                    .SetOnInsert(x => x.DueTime, dueTime)
                    .SetOnInsert(x => x.Id, Guid.NewGuid().ToString())
                    .SetOnInsert(x => x.Key, key)
                    .SetOnInsert(x => x.Progressing, false)
                    .SetOnInsert(x => x.ProgressingStarted, null)
                    .SetOnInsert(x => x.RetryCount, retryCount)
                    .SetOnInsert(x => x.Jobs, new List<T> { job }),
                Upsert, ct);
        }

        public Task CompleteAsync(string id)
        {
            return Collection.DeleteOneAsync(x => x.Id == id);
        }

        public Task CompleteByKeyAsync(string key)
        {
            return Collection.DeleteOneAsync(x => x.Key == key);
        }
    }
}