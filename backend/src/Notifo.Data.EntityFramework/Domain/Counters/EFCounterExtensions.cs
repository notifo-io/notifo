// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Notifo.Infrastructure;

namespace Notifo.Domain.Counters;

public static class EFCounterExtensions
{
    private const int MaxAttempts = 20;

    public static async Task WriteCountersAsync<TEntity>(this DbContext dbContext, string id, CounterMap values, Func<TEntity>? create,
        CancellationToken ct) where TEntity : class, IEFCounterEntity
    {
        if (values is not { Count: > 0 })
        {
            return;
        }

        var set = dbContext.Set<TEntity>();

        // The counters are stored in their own column with their own version, so that counter updates do not change the etag.
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var current =
                await set.Where(x => x.DocId == id)
                    .Select(x => new { x.Counters, x.CountersVersion })
                    .FirstOrDefaultAsync(ct);

            if (current == null)
            {
                if (create == null)
                {
                    return;
                }

                var entity = create();

                entity.Counters = new CounterMap(values);
                try
                {
                    await set.AddAsync(entity, ct);
                    await dbContext.SaveChangesAsync(ct);
                    return;
                }
                catch (DbUpdateException ex) when (ex.IsUniqueViolation())
                {
                    // The entity has been created in the meantime.
                    continue;
                }
                finally
                {
                    dbContext.Entry(entity).State = EntityState.Detached;
                }
            }

            var counters = current.Counters != null ? new CounterMap(current.Counters) : [];

            foreach (var (key, value) in values)
            {
                counters[key] = counters.GetValueOrDefault(key) + value;
            }

            var version = current.CountersVersion;

            var updated =
                await set.Where(x => x.DocId == id && x.CountersVersion == version)
                    .ExecuteUpdateAsync(u => u
                        .SetProperty(x => x.Counters, counters)
                        .SetProperty(x => x.CountersVersion, version + 1),
                        ct);

            if (updated == 1)
            {
                return;
            }
        }

        throw new InvalidOperationException($"Failed to write counters for '{id}' after {MaxAttempts} attempts.");
    }
}
