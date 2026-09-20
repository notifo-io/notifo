// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;

namespace Notifo.Infrastructure.KeyValueStore;

public sealed class EFKeyValueStore<TContext>(IDbContextFactory<TContext> dbContextFactory) : IKeyValueStore where TContext : DbContext
{
    public async Task<string?> GetAsync(string key,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(key);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        return await dbContext.Set<EFKeyValueEntity>().Where(x => x.Key == key).Select(x => x.Value).FirstOrDefaultAsync(ct);
    }

    public async Task<bool> RemvoveAsync(string key,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(key);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        var deleted = await dbContext.Set<EFKeyValueEntity>().Where(x => x.Key == key).ExecuteDeleteAsync(ct);

        return deleted == 1;
    }

    public async Task<bool> SetAsync(string key, string? value,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(key);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        var entity = new EFKeyValueEntity { Key = key, Value = value };

        // Returns true when an existing value has been modified.
        return await dbContext.UpsertAsync(entity, x => x.Key == key, u => u.SetProperty(x => x.Value, value), ct);
    }

    public async Task<string?> SetIfNotExistsAsync(string key, string? value,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(key);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        if (await InsertAsync(dbContext, key, value, ct))
        {
            return value;
        }

        return await dbContext.Set<EFKeyValueEntity>().Where(x => x.Key == key).Select(x => x.Value).FirstOrDefaultAsync(ct);
    }

    private static async Task<bool> InsertAsync(TContext dbContext, string key, string? value,
        CancellationToken ct)
    {
        var entity = new EFKeyValueEntity { Key = key, Value = value };
        try
        {
            await dbContext.Set<EFKeyValueEntity>().AddAsync(entity, ct);
            await dbContext.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            return false;
        }
        finally
        {
            dbContext.Entry(entity).State = EntityState.Detached;
        }
    }
}
