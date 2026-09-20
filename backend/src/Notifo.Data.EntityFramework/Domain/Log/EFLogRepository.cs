// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using NodaTime;
using Notifo.Infrastructure;
using PhenX.EntityFrameworkCore.BulkInsert.Options;

namespace Notifo.Domain.Log;

public sealed class EFLogRepository<TContext>(IDbContextFactory<TContext> dbContextFactory) : ILogRepository where TContext : DbContext
{
    public async Task<IResultList<LogEntry>> QueryAsync(string appId, LogQuery query,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("EFLogRepository/QueryAsync"))
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

            var filtered = dbContext.Set<EFLogEntity>().Where(x => x.AppId == appId);

            if (!string.IsNullOrWhiteSpace(query.UserId))
            {
                filtered = filtered.Where(x => x.UserId == query.UserId);
            }
            else
            {
                filtered = filtered.Where(x => x.UserId == null);
            }

            filtered = filtered.WhereContainsIgnoreCase(query.Query, x => x.Message);

            if (query.Systems?.Length > 0)
            {
                var systems = query.Systems;

                filtered = filtered.Where(x => systems.Contains(x.System));
            }

            if (query.EventCode > 0)
            {
                filtered = filtered.Where(x => x.EventCode == query.EventCode);
            }

            return await filtered.ToResultListAsync(filtered.OrderByDescending(x => x.LastSeen), query, x => x.ToEntry(), activity, ct);
        }
    }

    public async Task<IResultList<LogEntry>> BatchWriteAsync(IEnumerable<(LogWrite Write, int Count, Instant Now)> updates,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("EFLogRepository/BatchWriteAsync"))
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

            // Use a token to track which entries have just been written.
            var writeId = Guid.NewGuid().ToString();

            var entities = new Dictionary<string, EFLogEntity>();

            // The same entry must not be written twice with a single statement, therefore we merge them first.
            foreach (var (write, count, now) in updates)
            {
                var (appId, userId, eventCode, message, system) = write;

                var id = EFLogEntity.CreateId(appId, userId, eventCode, message, system);
                if (entities.TryGetValue(id, out var existing))
                {
                    existing.Count += count;
                    existing.LastSeen = now;
                    continue;
                }

                entities[id] = new EFLogEntity
                {
                    AppId = appId,
                    Count = count,
                    EventCode = eventCode,
                    FirstSeen = now,
                    FirstWriteId = writeId,
                    Id = id,
                    LastSeen = now,
                    Message = message.ToMaxLength(FieldLengths.LongText)!,
                    System = system,
                    UserId = userId,
                };
            }

            if (dbContext.Database.IsMySql())
            {
                // The bulk library cannot reference the existing values with MySQL.
                foreach (var entity in entities.Values)
                {
                    await UpsertAsync(dbContext, entity, ct);
                }
            }
            else
            {
                await dbContext.BulkUpsertAsync(entities.Values.ToList(), new OnConflictOptions<EFLogEntity>
                {
                    Update = (inserted, excluded) => new EFLogEntity
                    {
                        Count = inserted.Count + excluded.Count,
                        LastSeen = excluded.LastSeen,
                    },
                }, ct);
            }

            // Every log entry with the first write token has been just created.
            var resultItems =
                await dbContext.Set<EFLogEntity>()
                    .Where(x => x.FirstWriteId == writeId)
                    .ToListAsync(ct);

            activity?.SetTag("numNewEntries", resultItems.Count);

            return ResultList.Create(resultItems.Count, resultItems.Select(x => x.ToEntry()));
        }
    }

    private static Task UpsertAsync(TContext dbContext, EFLogEntity entity,
        CancellationToken ct)
    {
        return dbContext.UpsertAsync(entity, x => x.Id == entity.Id, u => u
            .SetProperty(x => x.Count, x => x.Count + entity.Count)
            .SetProperty(x => x.LastSeen, entity.LastSeen),
            ct);
    }
}
