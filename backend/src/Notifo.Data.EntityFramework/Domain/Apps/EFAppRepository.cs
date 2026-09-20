// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;

namespace Notifo.Domain.Apps;

public sealed class EFAppRepository<TContext>(IDbContextFactory<TContext> dbContextFactory)
    : EFStore<TContext, EFAppEntity, App>(dbContextFactory), IAppRepository where TContext : DbContext
{
    public async IAsyncEnumerable<App> QueryAllAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await foreach (var entity in dbContext.Set<EFAppEntity>().AsAsyncEnumerable().WithCancellation(ct))
        {
            yield return entity.ToApp();
        }
    }

    public async Task<List<App>> QueryWithPendingIntegrationsAsync(
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAppRepository/QueryWithPendingIntegrationsAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var entities =
                await dbContext.Set<EFAppEntity>()
                    .Where(x => x.IsPending)
                    .ToListAsync(ct);

            return entities.Select(x => x.ToApp()).ToList();
        }
    }

    public async Task<List<App>> QueryAsync(string contributorId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAppRepository/QueryAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var appIds = dbContext.Set<EFAppContributorEntity>().Where(x => x.ContributorId == contributorId).Select(x => x.AppId);

            var entities =
                await dbContext.Set<EFAppEntity>()
                    .Where(x => appIds.Contains(x.DocId))
                    .ToListAsync(ct);

            return entities.Select(x => x.ToApp()).ToList();
        }
    }

    public async Task<(App? App, string? Etag)> GetByApiKeyAsync(string apiKey,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAppRepository/GetByApiKeyAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var appIds = dbContext.Set<EFAppApiKeyEntity>().Where(x => x.ApiKey == apiKey).Select(x => x.AppId);

            var entity =
                await dbContext.Set<EFAppEntity>()
                    .Where(x => appIds.Contains(x.DocId))
                    .FirstOrDefaultAsync(ct);

            return (entity?.ToApp(), entity?.Etag);
        }
    }

    public async Task<(App? App, string? Etag)> GetByAuthDomainAsync(string domain,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAppRepository/GetByAuthDomainAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var entity =
                await dbContext.Set<EFAppEntity>()
                    .Where(x => x.AuthDomain == domain)
                    .FirstOrDefaultAsync(ct);

            return (entity?.ToApp(), entity?.Etag);
        }
    }

    public async Task<(App? App, string? Etag)> GetAsync(string id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAppRepository/GetAsync"))
        {
            var entity = await GetDocumentAsync(id, ct);

            return (entity?.ToApp(), entity?.Etag);
        }
    }

    public async Task<bool> AnyAuthDomainAsync(
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAppRepository/AnyAuthDomainAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            return await dbContext.Set<EFAppEntity>().AnyAsync(x => x.AuthDomain != null, ct);
        }
    }

    public async Task UpsertAsync(App app, string? oldEtag = null,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAppRepository/UpsertAsync"))
        {
            var entity = EFAppEntity.FromApp(app);

            await using var dbContext = await CreateDbContextAsync(ct);
            await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

            // Read the old values before the row is locked to keep the transaction short.
            var oldContributors =
                await dbContext.Set<EFAppContributorEntity>()
                    .Where(x => x.AppId == entity.DocId).Select(x => x.ContributorId)
                    .ToListAsync(ct);

            var oldApiKeys =
                await dbContext.Set<EFAppApiKeyEntity>()
                    .Where(x => x.AppId == entity.DocId).Select(x => x.ApiKey)
                    .ToListAsync(ct);

            await UpsertDocumentAsync(dbContext, entity, oldEtag, ct);

            var removedContributors = oldContributors.Except(app.Contributors.Keys).ToList();
            if (removedContributors.Count > 0)
            {
                await dbContext.Set<EFAppContributorEntity>()
                    .Where(x => x.AppId == entity.DocId && removedContributors.Contains(x.ContributorId))
                    .ExecuteDeleteAsync(ct);
            }

            var addedContributors =
                app.Contributors.Keys.Except(oldContributors)
                    .Select(x => new EFAppContributorEntity { AppId = entity.DocId, ContributorId = x })
                    .ToList();

            await dbContext.BulkInsertAsync(addedContributors, ct);

            var removedApiKeys = oldApiKeys.Except(app.ApiKeys.Keys).ToList();
            if (removedApiKeys.Count > 0)
            {
                await dbContext.Set<EFAppApiKeyEntity>()
                    .Where(x => x.AppId == entity.DocId && removedApiKeys.Contains(x.ApiKey))
                    .ExecuteDeleteAsync(ct);
            }

            var addedApiKeys =
                app.ApiKeys.Keys.Except(oldApiKeys)
                    .Select(x => new EFAppApiKeyEntity { ApiKey = x, AppId = entity.DocId })
                    .ToList();
            try
            {
                // The API keys have their own table, because they must be unique over all apps.
                await dbContext.BulkInsertAsync(addedApiKeys, ct);
            }
            catch (Exception ex) when (ex.IsUniqueViolation())
            {
                throw new UniqueConstraintException();
            }

            await transaction.CommitAsync(ct);
        }
    }

    public async Task DeleteAsync(string id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAppRepository/DeleteAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);
            await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

            await dbContext.Set<EFAppContributorEntity>().Where(x => x.AppId == id)
                .ExecuteDeleteAsync(ct);

            await dbContext.Set<EFAppApiKeyEntity>().Where(x => x.AppId == id)
                .ExecuteDeleteAsync(ct);

            await dbContext.Set<EFAppEntity>().Where(x => x.DocId == id)
                .ExecuteDeleteAsync(ct);

            await transaction.CommitAsync(ct);
        }
    }

    public async Task BatchWriteAsync(List<(string Key, CounterMap Counters)> counters,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFAppRepository/BatchWriteAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            foreach (var (id, values) in counters)
            {
                await dbContext.WriteCountersAsync<EFAppEntity>(id, values, null, ct);
            }
        }
    }

    protected override void BuildUpdate(UpdateSettersBuilder<EFAppEntity> update, EFAppEntity entity)
    {
        update.SetProperty(x => x.AuthDomain, entity.AuthDomain);
        update.SetProperty(x => x.IsPending, entity.IsPending);
    }
}
