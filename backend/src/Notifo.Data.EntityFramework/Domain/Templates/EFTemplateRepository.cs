// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Notifo.Infrastructure;

namespace Notifo.Domain.Templates;

public sealed class EFTemplateRepository<TContext>(IDbContextFactory<TContext> dbContextFactory)
    : EFStore<TContext, EFTemplateEntity, Template>(dbContextFactory), ITemplateRepository where TContext : DbContext
{
    public async Task<IResultList<Template>> QueryAsync(string appId, TemplateQuery query,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("EFTemplateRepository/QueryAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var filtered = dbContext.Set<EFTemplateEntity>().Where(x => x.AppId == appId);

            filtered = filtered.WhereContainsIgnoreCase(query.Query, x => x.Code);

            return await filtered.ToResultListAsync(filtered.OrderBy(x => x.Code), query, x => x.ToTemplate(), activity, ct);
        }
    }

    public async Task<(Template? Template, string? Etag)> GetAsync(string appId, string code,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFTemplateRepository/GetAsync"))
        {
            var entity = await GetDocumentAsync(EFTemplateEntity.CreateId(appId, code), ct);

            return (entity?.ToTemplate(), entity?.Etag);
        }
    }

    public async Task UpsertAsync(Template template, string? oldEtag = null,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFTemplateRepository/UpsertAsync"))
        {
            await UpsertDocumentAsync(EFTemplateEntity.FromTemplate(template), oldEtag, ct);
        }
    }

    public async Task DeleteAsync(string appId, string code,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFTemplateRepository/DeleteAsync"))
        {
            await DeleteDocumentAsync(EFTemplateEntity.CreateId(appId, code), ct);
        }
    }
}
