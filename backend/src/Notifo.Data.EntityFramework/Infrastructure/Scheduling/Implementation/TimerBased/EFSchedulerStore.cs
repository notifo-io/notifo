// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Notifo.Infrastructure.Scheduling.Implementation.TimerBased;

public sealed class EFSchedulerStore<TContext, T>(IDbContextFactory<TContext> dbContextFactory, JsonSerializerOptions jsonOptions, SchedulerOptions options)
    : ISchedulerStore<T> where TContext : DbContext
{
    private const int MaxAttempts = 20;
    private readonly string queueName = options.QueueName.ToLowerInvariant();

    public async Task<SchedulerBatch<T>?> DequeueAsync(Instant time,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFSchedulerStore/DequeueAsync"))
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

            // Claim a batch with a conditional update, because another worker could claim the same batch.
            for (var attempt = 0; attempt < MaxAttempts; attempt++)
            {
                var id =
                    await Batches(dbContext)
                        .Where(x => !x.Progressing && x.DueTime <= time)
                        .OrderBy(x => x.DueTime)
                        .Select(x => x.Id)
                        .FirstOrDefaultAsync(ct);

                if (id == null)
                {
                    return null;
                }

                var claimed =
                    await Batches(dbContext)
                        .Where(x => x.Id == id && !x.Progressing)
                        .ExecuteUpdateAsync(u => u
                            .SetProperty(x => x.Progressing, true)
                            .SetProperty(x => x.ProgressingStarted, time)
                            .SetProperty(x => x.Version, x => x.Version + 1),
                            ct);

                if (claimed == 1)
                {
                    var entity = await Batches(dbContext).Where(x => x.Id == id).FirstOrDefaultAsync(ct);

                    return entity != null ? ToBatch(entity) : null;
                }
            }

            return null;
        }
    }

    public async Task ResetDeadAsync(Instant oldTime, Instant next,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFSchedulerStore/ResetDeadAsync"))
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

            await Batches(dbContext)
                .Where(x => x.Progressing && x.ProgressingStarted < oldTime)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.DueTime, next)
                    .SetProperty(x => x.Progressing, false)
                    .SetProperty(x => x.ProgressingStarted, (Instant?)null)
                    .SetProperty(x => x.RetryCount, x => x.RetryCount + 1)
                    .SetProperty(x => x.Version, x => x.Version + 1),
                    ct);
        }
    }

    public async Task RetryAsync(string id, Instant next,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFSchedulerStore/RetryAsync"))
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

            await Batches(dbContext)
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.DueTime, next)
                    .SetProperty(x => x.Progressing, false)
                    .SetProperty(x => x.ProgressingStarted, (Instant?)null)
                    .SetProperty(x => x.RetryCount, x => x.RetryCount + 1)
                    .SetProperty(x => x.Version, x => x.Version + 1),
                    ct);
        }
    }

    public async Task EnqueueGroupedAsync(string key, string groupKey, T job, Instant dueTime, int retryCount = 0,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFSchedulerStore/EnqueueGroupedAsync"))
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

            // The batch must not be handled before the delay of the last job has been elapsed.
            await EnqueueAsync(dbContext, key, groupKey, job, retryCount, dueTime,
                x => x.GroupKey == groupKey && !x.Progressing && x.DueTime <= dueTime,
                x => x > dueTime ? x : dueTime,
                ct);
        }
    }

    public async Task EnqueueAsync(string key, T job, Instant dueTime, int retryCount = 0,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFSchedulerStore/EnqueueScheduledAsync"))
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

            // A job can be scheduled again with an earlier due time, e.g. for updates.
            await EnqueueAsync(dbContext, key, key, job, retryCount, dueTime,
                x => x.GroupKey == key && !x.Progressing,
                x => x < dueTime ? x : dueTime,
                ct);
        }
    }

    private async Task EnqueueAsync(TContext dbContext, string key, string groupKey, T job, int retryCount, Instant dueTime,
        Expression<Func<EFSchedulerEntity, bool>> filter,
        Func<Instant, Instant> mergeDueTime,
        CancellationToken ct)
    {
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var existing = await Batches(dbContext).Where(filter).FirstOrDefaultAsync(ct);
            if (existing == null)
            {
                var entity = new EFSchedulerEntity
                {
                    DueTime = dueTime,
                    GroupKey = groupKey,
                    Id = Guid.NewGuid().ToString(),
                    Jobs = SerializeJobs(new Dictionary<string, T> { [key] = job }),
                    Progressing = false,
                    ProgressingStarted = null,
                    QueueName = queueName,
                    RetryCount = retryCount,
                    Version = 0,
                };

                try
                {
                    await dbContext.Set<EFSchedulerEntity>().AddAsync(entity, ct);
                    await dbContext.SaveChangesAsync(ct);
                    return;
                }
                finally
                {
                    dbContext.Entry(entity).State = EntityState.Detached;
                }
            }

            var jobs = DeserializeJobs(existing.Jobs);

            jobs[key] = job;

            var newJobs = SerializeJobs(jobs);
            var newDueTime = mergeDueTime(existing.DueTime);

            if (await UpdateAsync(dbContext, existing, newJobs, newDueTime, ct))
            {
                return;
            }
        }

        throw new InvalidOperationException($"Failed to enqueue job after {MaxAttempts} attempts.");
    }

    public async Task CompleteAsync(string id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFSchedulerStore/CompleteAsync"))
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

            await Batches(dbContext).Where(x => x.Id == id)
                .ExecuteDeleteAsync(ct);
        }
    }

    public async Task<bool> CompleteByKeyAsync(string key,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFSchedulerStore/CompleteByKeyAsync"))
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

            var id = await Batches(dbContext).Where(x => x.GroupKey == key).Select(x => x.Id).FirstOrDefaultAsync(ct);
            if (id == null)
            {
                return false;
            }

            var deleted = await Batches(dbContext).Where(x => x.Id == id).ExecuteDeleteAsync(ct);

            return deleted == 1;
        }
    }

    public async Task<bool> CompleteByKeyAsync(string key, string groupKey,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFSchedulerStore/CompleteByKeyAsync"))
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

            for (var attempt = 0; attempt < MaxAttempts; attempt++)
            {
                var candidates =
                    await Batches(dbContext)
                        .Where(x => x.GroupKey == groupKey && !x.Progressing)
                        .ToListAsync(ct);

                // Multiple batches can have the same group key, therefore we have to find the batch that contains the job.
                var (batch, jobs) =
                    candidates
                        .Select(x => (Batch: x, Jobs: DeserializeJobs(x.Jobs)))
                        .FirstOrDefault(x => x.Jobs.ContainsKey(key));

                if (batch == null)
                {
                    return false;
                }

                jobs.Remove(key);

                if (jobs.Count == 0)
                {
                    // Only delete the batch if it is still unchanged, because other jobs could have been added in the meantime.
                    var deleted =
                        await Batches(dbContext)
                            .Where(x => x.Id == batch.Id && x.Version == batch.Version && !x.Progressing)
                            .ExecuteDeleteAsync(ct);

                    if (deleted == 1)
                    {
                        return true;
                    }
                }
                else if (await UpdateAsync(dbContext, batch, SerializeJobs(jobs), batch.DueTime, ct))
                {
                    return true;
                }
            }

            return false;
        }
    }

    private static async Task<bool> UpdateAsync(TContext dbContext, EFSchedulerEntity batch, string jobs, Instant dueTime,
        CancellationToken ct)
    {
        var version = batch.Version;

        var updated =
            await dbContext.Set<EFSchedulerEntity>()
                .Where(x => x.Id == batch.Id && x.Version == version && !x.Progressing)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.Jobs, jobs)
                    .SetProperty(x => x.DueTime, dueTime)
                    .SetProperty(x => x.Version, version + 1),
                    ct);

        return updated == 1;
    }

    private IQueryable<EFSchedulerEntity> Batches(TContext dbContext)
    {
        return dbContext.Set<EFSchedulerEntity>().Where(x => x.QueueName == queueName);
    }

    private SchedulerBatch<T> ToBatch(EFSchedulerEntity entity)
    {
        return new SchedulerBatch<T>
        {
            Id = entity.Id,
            DueTime = entity.DueTime,
            GroupKey = entity.GroupKey,
            JobsV2 = DeserializeJobs(entity.Jobs),
            Progressing = entity.Progressing,
            ProgressingStarted = entity.ProgressingStarted,
            RetryCount = entity.RetryCount,
        };
    }

    private string SerializeJobs(Dictionary<string, T> jobs)
    {
        return JsonSerializer.Serialize(jobs, jsonOptions);
    }

    private Dictionary<string, T> DeserializeJobs(string jobs)
    {
        return JsonSerializer.Deserialize<Dictionary<string, T>>(jobs, jsonOptions) ?? [];
    }
}
