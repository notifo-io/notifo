// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Notifo.Infrastructure;

namespace Notifo.Domain.Media;

public sealed class EFMediaRepository<TContext>(IDbContextFactory<TContext> dbContextFactory)
    : EFStore<TContext, EFMediaEntity, Media>(dbContextFactory), IMediaRepository where TContext : DbContext
{
    public async Task<IResultList<Media>> QueryAsync(string appId, MediaQuery query,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("EFMediaRepository/QueryAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var filtered = dbContext.Set<EFMediaEntity>().Where(x => x.AppId == appId);

            filtered = filtered.WhereContainsIgnoreCase(query.Query, x => x.FileName);

            return await filtered.ToResultListAsync(filtered.OrderByDescending(x => x.LastUpdate), query, x => x.ToMedia(), activity, ct);
        }
    }

    public async Task<Media?> GetAsync(string appId, string fileName,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFMediaRepository/GetAsync"))
        {
            var entity = await GetDocumentAsync(EFMediaEntity.CreateId(appId, fileName), ct);

            return entity?.ToMedia();
        }
    }

    public async Task UpsertAsync(Media media,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFMediaRepository/UpsertAsync"))
        {
            await UpsertDocumentAsync(EFMediaEntity.FromMedia(media), null, ct);
        }
    }

    public async Task DeleteAsync(string appId, string fileName,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFMediaRepository/DeleteAsync"))
        {
            await DeleteDocumentAsync(EFMediaEntity.CreateId(appId, fileName), ct);
        }
    }

    protected override void BuildUpdate(UpdateSettersBuilder<EFMediaEntity> update, EFMediaEntity entity)
    {
        update.SetProperty(x => x.LastUpdate, entity.LastUpdate);
    }
}
