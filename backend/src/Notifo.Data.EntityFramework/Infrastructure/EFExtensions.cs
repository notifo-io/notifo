// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using PhenX.EntityFrameworkCore.BulkInsert.Extensions;
using PhenX.EntityFrameworkCore.BulkInsert.Options;

namespace Notifo.Infrastructure;

public static class EFExtensions
{
    public static DbContextOptionsBuilder SetDefaults(this DbContextOptionsBuilder builder)
    {
        builder.ConfigureWarnings(w => w.Ignore(CoreEventId.CollectionWithoutComparer));

        // Almost everything is read only or written with bulk updates, so tracking would only
        // cost a snapshot of every entity that is read. This cannot be done in OnConfiguring,
        // because the contexts are pooled and pooling forbids to modify the options there.
        builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        return builder;
    }

    public static async Task BulkInsertAsync<T>(this DbContext dbContext, List<T> source,
        CancellationToken ct) where T : class
    {
        if (source.Count == 0)
        {
            return;
        }

        // The bulk insert is much slower for single rows, because it also creates a temporary table.
        if (source.Count == 1)
        {
            var entity = source[0];
            try
            {
                await dbContext.AddAsync(entity, ct);
                await dbContext.SaveChangesAsync(ct);
            }
            finally
            {
                dbContext.Entry(entity).State = EntityState.Detached;
            }

            return;
        }

        await dbContext.ExecuteBulkInsertAsync(source, cancellationToken: ct);
    }

    public static async Task<bool> UpsertAsync<T>(this DbContext dbContext, T entity, Expression<Func<T, bool>> filter, Action<UpdateSettersBuilder<T>> update,
        CancellationToken ct) where T : class
    {
        // Query the entity first, because an update to a missing row locks a range in some databases and deadlocks with concurrent inserts.
        var exists = await dbContext.Set<T>().AnyAsync(filter, ct);
        if (exists && await dbContext.UpdateAsync(filter, update, ct) > 0)
        {
            return true;
        }

        try
        {
            await dbContext.AddAsync(entity, ct);
            await dbContext.SaveChangesAsync(ct);
            return false;
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            // The entity has been inserted in the meantime or another unique index is violated.
            if (await dbContext.UpdateAsync(filter, update, ct) == 0)
            {
                throw new UniqueConstraintException();
            }

            return true;
        }
        finally
        {
            dbContext.Entry(entity).State = EntityState.Detached;
        }
    }

    public static async Task<int> UpdateAsync<T>(this DbContext dbContext, Expression<Func<T, bool>> filter, Action<UpdateSettersBuilder<T>> update,
        CancellationToken ct) where T : class
    {
        try
        {
            return await dbContext.Set<T>().Where(filter).ExecuteUpdateAsync(update, ct);
        }
        catch (Exception ex) when (ex.IsUniqueViolation())
        {
            throw new UniqueConstraintException();
        }
    }

    public static Task BulkUpsertAsync<T>(this DbContext dbContext, List<T> source, OnConflictOptions<T> onConflict,
        CancellationToken ct) where T : class
    {
        if (source.Count == 0)
        {
            return Task.CompletedTask;
        }

        return dbContext.ExecuteBulkInsertAsync(source, o => { }, onConflict, ct);
    }

    private static IQueryable<T> Page<T>(this IQueryable<T> source, QueryBase query)
    {
        if (query.Skip > 0)
        {
            source = source.Skip(query.Skip);
        }

        if (query.Take is > 0 and < int.MaxValue)
        {
            source = source.Take(query.Take);
        }

        return source;
    }

    public static async Task<IResultList<TResult>> ToResultListAsync<T, TResult>(this IQueryable<T> filtered, IQueryable<T> sorted, QueryBase query, Func<T, TResult> map,
        Activity? activity,
        CancellationToken ct)
    {
        var resultItems = await sorted.Page(query).ToListAsync(ct);
        var resultTotal = (long)resultItems.Count;

        if (query.ShouldQueryTotal(resultItems))
        {
            resultTotal = await filtered.LongCountAsync(ct);
        }

        activity?.SetTag("numResults", resultItems.Count);
        activity?.SetTag("numTotal", resultTotal);

        return ResultList.Create(resultTotal, resultItems.Select(map));
    }
}
