// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Notifo.Infrastructure;

public abstract class EFStore<TContext, TEntity, T>(IDbContextFactory<TContext> dbContextFactory)
    where TContext : DbContext
    where TEntity : EFEntity<T>
    where T : class
{
    protected Task<TContext> CreateDbContextAsync(CancellationToken ct)
    {
        return dbContextFactory.CreateDbContextAsync(ct);
    }

    protected async Task<TEntity?> GetDocumentAsync(string id,
        CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        return await GetDocumentAsync(dbContext, id, ct);
    }

    protected static async Task<TEntity?> GetDocumentAsync(TContext dbContext, string id,
        CancellationToken ct)
    {
        Guard.NotNullOrEmpty(id);

        return await dbContext.Set<TEntity>().Where(x => x.DocId == id).FirstOrDefaultAsync(ct);
    }

    protected async Task InsertDocumentAsync(TEntity entity,
        CancellationToken ct)
    {
        Guard.NotNull(entity);
        Guard.NotNullOrEmpty(entity.DocId);

        await using var dbContext = await CreateDbContextAsync(ct);
        try
        {
            await dbContext.Set<TEntity>().AddAsync(entity, ct);
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            throw new UniqueConstraintException();
        }
    }

    protected async Task UpsertDocumentAsync(TEntity entity, string? oldEtag,
        CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await UpsertDocumentAsync(dbContext, entity, oldEtag, ct);
    }

    protected async Task UpsertDocumentAsync(TContext dbContext, TEntity entity, string? oldEtag,
        CancellationToken ct)
    {
        Guard.NotNull(entity);
        Guard.NotNullOrEmpty(entity.DocId);

        var id = entity.DocId;
        var update = CreateUpdate(entity);

        if (!string.IsNullOrWhiteSpace(oldEtag))
        {
            // Do not insert the document again, because it could have been deleted in the meantime.
            var updated = await dbContext.UpdateAsync<TEntity>(x => x.DocId == id && x.Etag == oldEtag, update, ct);
            if (updated == 0)
            {
                var currentEtag =
                    await dbContext.Set<TEntity>()
                        .Where(x => x.DocId == id).Select(x => x.Etag)
                        .FirstOrDefaultAsync(ct);

                throw new InconsistentStateException(currentEtag ?? string.Empty, oldEtag);
            }

            return;
        }

        await dbContext.UpsertAsync(entity, x => x.DocId == id, update, ct);
    }

    protected async Task DeleteDocumentAsync(string id,
        CancellationToken ct)
    {
        Guard.NotNullOrEmpty(id);

        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<TEntity>().Where(x => x.DocId == id)
            .ExecuteDeleteAsync(ct);
    }

    private Action<UpdateSettersBuilder<TEntity>> CreateUpdate(TEntity entity)
    {
        return u =>
        {
            u.SetProperty(x => x.Doc, entity.Doc);
            u.SetProperty(x => x.Etag, entity.Etag);

            BuildUpdate(u, entity);
        };
    }

    protected virtual void BuildUpdate(UpdateSettersBuilder<TEntity> update, TEntity entity)
    {
    }
}
