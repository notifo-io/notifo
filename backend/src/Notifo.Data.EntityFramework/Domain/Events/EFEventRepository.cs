// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Timers;
using Squidex.Hosting;

namespace Notifo.Domain.Events;

public sealed class EFEventRepository<TContext>(IDbContextFactory<TContext> dbContextFactory, IOptions<EventsOptions> options, IClock clock)
    : EFStore<TContext, EFEventEntity, Event>(dbContextFactory), IEventRepository, IInitializable where TContext : DbContext
{
    private readonly TimeSpan retentionTime = options.Value.RetentionTime;
    private CompletionTimer? timer;

    public Task InitializeAsync(
        CancellationToken ct)
    {
        timer = new CompletionTimer((int)TimeSpan.FromMinutes(10).TotalMilliseconds, CleanupAsync);
        return Task.CompletedTask;
    }

    public Task ReleaseAsync(
        CancellationToken ct)
    {
        return timer?.StopAsync() ?? Task.CompletedTask;
    }

    public async Task CleanupAsync(
        CancellationToken ct)
    {
        var maxAge = clock.GetCurrentInstant().Minus(Duration.FromTimeSpan(retentionTime));

        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFEventEntity>().Where(x => x.Created < maxAge)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<IResultList<Event>> QueryAsync(string appId, EventQuery query,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("EFEventRepository/QueryAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var filtered = dbContext.Set<EFEventEntity>().Where(x => x.AppId == appId);

            filtered = filtered.WhereContainsIgnoreCase(query.Query, x => x.Topic, x => x.SearchText);

            filtered = filtered.WhereContainsAnyTag(x => x.SendChannels, query.Channels);

            return await filtered.ToResultListAsync(filtered.OrderByDescending(x => x.Created), query, x => x.ToEvent(), activity, ct);
        }
    }

    public async Task InsertAsync(Event @event,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFEventRepository/InsertAsync"))
        {
            var entity = EFEventEntity.FromEvent(@event);

            await InsertDocumentAsync(entity, ct);
        }
    }

    public async Task<bool> IsPendingAsync(string appId, string id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFEventRepository/IsPendingAsync"))
        {
            var docId = EFEventEntity.CreateId(appId, id);

            await using var dbContext = await CreateDbContextAsync(ct);

            return await dbContext.Set<EFEventEntity>().AnyAsync(x => x.DocId == docId && x.Pending, ct);
        }
    }

    public async Task MarkPublishedAsync(string appId, string id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFEventRepository/MarkPublishedAsync"))
        {
            var docId = EFEventEntity.CreateId(appId, id);

            await using var dbContext = await CreateDbContextAsync(ct);

            await dbContext.Set<EFEventEntity>().Where(x => x.DocId == docId)
                .ExecuteUpdateAsync(u => u.SetProperty(x => x.Pending, false), ct);
        }
    }

    public async Task BatchWriteAsync(List<((string AppId, string EventId) Key, CounterMap Counters)> counters,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFEventRepository/BatchWriteAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            foreach (var ((appId, id), values) in counters)
            {
                await dbContext.WriteCountersAsync<EFEventEntity>(EFEventEntity.CreateId(appId, id), values, null, ct);
            }
        }
    }
}
