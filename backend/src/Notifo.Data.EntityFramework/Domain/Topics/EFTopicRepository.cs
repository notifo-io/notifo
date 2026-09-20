// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NodaTime;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;

namespace Notifo.Domain.Topics;

public sealed class EFTopicRepository<TContext>(IDbContextFactory<TContext> dbContextFactory)
    : EFStore<TContext, EFTopicEntity, Topic>(dbContextFactory), ITopicRepository where TContext : DbContext
{
    public async Task<IResultList<Topic>> QueryAsync(string appId, TopicQuery query,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("EFTopicRepository/QueryAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var filtered = dbContext.Set<EFTopicEntity>().Where(x => x.AppId == appId);

            switch (query.Scope)
            {
                case TopicQueryScope.Explicit:
                    filtered = filtered.Where(x => x.IsExplicit);
                    break;
                case TopicQueryScope.Implicit:
                    filtered = filtered.Where(x => !x.IsExplicit);
                    break;
            }

            filtered = filtered.WhereContainsIgnoreCase(query.Query, x => x.Path);

            return await filtered.ToResultListAsync(filtered.OrderByDescending(x => x.LastUpdate), query, x => x.ToTopic(), activity, ct);
        }
    }

    public async Task<(Topic? Topic, string? Etag)> GetAsync(string appId, TopicId path,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFTopicRepository/GetAsync"))
        {
            var entity = await GetDocumentAsync(EFTopicEntity.CreateId(appId, path), ct);

            return (entity?.ToTopic(), entity?.Etag);
        }
    }

    public async Task UpsertAsync(Topic topic, string? oldEtag = null,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFTopicRepository/UpsertAsync"))
        {
            await UpsertDocumentAsync(EFTopicEntity.FromTopic(topic), oldEtag, ct);
        }
    }

    public async Task DeleteAsync(string appId, TopicId path,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFTopicRepository/DeleteAsync"))
        {
            await DeleteDocumentAsync(EFTopicEntity.CreateId(appId, path), ct);
        }
    }

    public async Task BatchWriteAsync(List<((string AppId, string Path) Key, CounterMap Counters)> counters,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFTopicRepository/BatchWriteAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var now = SystemClock.Instance.GetCurrentInstant();

            foreach (var ((appId, path), values) in counters)
            {
                // Topics are created implicitly when a notification is sent to them.
                await dbContext.WriteCountersAsync(EFTopicEntity.CreateId(appId, path), values, () =>
                {
                    return EFTopicEntity.FromTopic(new Topic(appId, path, now) { LastUpdate = now });
                }, ct);
            }
        }
    }

    protected override void BuildUpdate(UpdateSettersBuilder<EFTopicEntity> update, EFTopicEntity entity)
    {
        update.SetProperty(x => x.IsExplicit, entity.IsExplicit);
        update.SetProperty(x => x.LastUpdate, entity.LastUpdate);
    }
}
