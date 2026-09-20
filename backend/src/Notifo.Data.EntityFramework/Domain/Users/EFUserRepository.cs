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

namespace Notifo.Domain.Users;

public sealed class EFUserRepository<TContext>(IDbContextFactory<TContext> dbContextFactory)
    : EFStore<TContext, EFUserEntity, User>(dbContextFactory), IUserRepository where TContext : DbContext
{
    public async IAsyncEnumerable<string> QueryIdsAsync(string appId,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFUserRepository/QueryIdsAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var ids = dbContext.Set<EFUserEntity>().Where(x => x.AppId == appId).Select(x => x.UserId);

            await foreach (var id in ids.AsAsyncEnumerable().WithCancellation(ct))
            {
                yield return id;
            }
        }
    }

    public async Task<IResultList<User>> QueryAsync(string appId, UserQuery query,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("EFUserRepository/QueryAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var filtered = dbContext.Set<EFUserEntity>().Where(x => x.AppId == appId);

            filtered = filtered.WhereContainsIgnoreCase(query.Query, x => x.UserId, x => x.FullName, x => x.EmailAddress);

            return await filtered.ToResultListAsync(filtered.OrderBy(x => x.DocId), query, x => x.ToUser(), activity, ct);
        }
    }

    public async Task<(User? User, string? Etag)> GetByApiKeyAsync(string apiKey,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFUserRepository/GetByApiKeyAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var entity = await dbContext.Set<EFUserEntity>().Where(x => x.ApiKey == apiKey).FirstOrDefaultAsync(ct);

            return (entity?.ToUser(), entity?.Etag);
        }
    }

    public async Task<(User? User, string? Etag)> GetByPropertyAsync(string appId, string key, string value,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFUserRepository/GetByPropertyAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var userIds =
                dbContext.Set<EFUserPropertyEntity>()
                    .Where(x => x.AppId == appId && x.Key == key && x.Value == value)
                    .Select(x => x.UserDocId);

            var entity =
                await dbContext.Set<EFUserEntity>()
                    .Where(x => userIds.Contains(x.DocId))
                    .FirstOrDefaultAsync(ct);

            return (entity?.ToUser(), entity?.Etag);
        }
    }

    public async Task<(User? User, string? Etag)> GetAsync(string appId, string id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFUserRepository/GetAsync"))
        {
            var entity = await GetDocumentAsync(EFUserEntity.CreateId(appId, id), ct);

            return (entity?.ToUser(), entity?.Etag);
        }
    }

    public async Task UpsertAsync(User user, string? oldEtag = null,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFUserRepository/UpsertAsync"))
        {
            var entity = EFUserEntity.FromUser(user);

            await using var dbContext = await CreateDbContextAsync(ct);
            await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

            // The properties are only needed to find users by property. Longer keys and values cannot be indexed.
            var newProperties =
                user.Properties
                    .Where(x => x.Key.Length <= EFUserPropertyEntity.MaxKeyLength && x.Value != null && x.Value.Length <= EFUserPropertyEntity.MaxValueLength)
                    .ToDictionary(x => x.Key, x => x.Value);

            // Read the old values before the row is locked to keep the transaction short.
            var oldProperties =
                await dbContext.Set<EFUserPropertyEntity>()
                    .Where(x => x.UserDocId == entity.DocId)
                    .ToDictionaryAsync(x => x.Key, x => x.Value, ct);

            await UpsertDocumentAsync(dbContext, entity, oldEtag, ct);

            var changedKeys = oldProperties.Where(x => newProperties.GetValueOrDefault(x.Key) != x.Value).Select(x => x.Key).ToList();
            if (changedKeys.Count > 0)
            {
                await dbContext.Set<EFUserPropertyEntity>()
                    .Where(x => x.UserDocId == entity.DocId && changedKeys.Contains(x.Key))
                    .ExecuteDeleteAsync(ct);
            }

            var addedProperties =
                newProperties.Where(x => oldProperties.GetValueOrDefault(x.Key) != x.Value)
                    .Select(x => new EFUserPropertyEntity { AppId = user.AppId, Key = x.Key, UserDocId = entity.DocId, Value = x.Value })
                    .ToList();

            await dbContext.BulkInsertAsync(addedProperties, ct);

            await transaction.CommitAsync(ct);
        }
    }

    public async Task DeleteAsync(string appId, string id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFUserRepository/DeleteAsync"))
        {
            var docId = EFUserEntity.CreateId(appId, id);

            await using var dbContext = await CreateDbContextAsync(ct);
            await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

            await dbContext.Set<EFUserPropertyEntity>().Where(x => x.UserDocId == docId)
                .ExecuteDeleteAsync(ct);

            await dbContext.Set<EFUserEntity>().Where(x => x.DocId == docId)
                .ExecuteDeleteAsync(ct);

            await transaction.CommitAsync(ct);
        }
    }

    public async Task BatchWriteAsync(List<((string AppId, string UserId) Key, CounterMap Counters)> counters,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFUserRepository/BatchWriteAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            foreach (var ((appId, id), values) in counters)
            {
                await dbContext.WriteCountersAsync<EFUserEntity>(EFUserEntity.CreateId(appId, id), values, null, ct);
            }
        }
    }

    protected override void BuildUpdate(UpdateSettersBuilder<EFUserEntity> update, EFUserEntity entity)
    {
        update.SetProperty(x => x.ApiKey, entity.ApiKey);
        update.SetProperty(x => x.EmailAddress, entity.EmailAddress);
        update.SetProperty(x => x.FullName, entity.FullName);
    }
}
