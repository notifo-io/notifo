// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Notifo.Infrastructure;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Notifo.Domain.ChannelTemplates;

public sealed class EFChannelTemplateRepository<TContext, T>(IDbContextFactory<TContext> dbContextFactory, JsonSerializerOptions jsonOptions)
    : EFStore<TContext, EFChannelTemplateEntity, string>(dbContextFactory), IChannelTemplateRepository<T> where TContext : DbContext where T : class
{
    private static readonly string TypeName = typeof(T).Name;

    public async Task<IResultList<ChannelTemplate<T>>> QueryAsync(string appId, ChannelTemplateQuery query,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("EFChannelTemplateRepository/QueryAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var filtered = dbContext.Set<EFChannelTemplateEntity>().Where(x => x.AppId == appId && x.Type == TypeName);

            filtered = filtered.WhereContainsIgnoreCase(query.Query, x => x.Name);

            return await filtered.ToResultListAsync(filtered.OrderBy(x => x.DocId), query, x => x.ToChannelTemplate<T>(jsonOptions), activity, ct);
        }
    }

    public async Task<ChannelTemplate<T>?> GetBestAsync(string appId, string? name,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFChannelTemplateRepository/GetBestAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var templates =
                await dbContext.Set<EFChannelTemplateEntity>()
                    .Where(x => x.AppId == appId && x.Type == TypeName)
                    .Select(x => new { x.DocId, x.Name, x.Primary })
                    .ToListAsync(ct);

            string? id;

            if (!string.IsNullOrWhiteSpace(name))
            {
                var truncated = name.ToMaxLength(FieldLengths.Text);

                id = templates.Find(x => x.Name == truncated)?.DocId;
            }
            else
            {
                id = templates.Find(x => x.Primary)?.DocId;
                if (id == null && templates.Count == 1)
                {
                    id = templates[0].DocId;
                }
            }

            if (id == null)
            {
                return null;
            }

            var entity = await GetDocumentAsync(dbContext, id, ct);

            return entity?.ToChannelTemplate<T>(jsonOptions);
        }
    }

    public async Task<(ChannelTemplate<T>? Template, string? Etag)> GetAsync(string appId, string code,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFChannelTemplateRepository/GetAsync"))
        {
            var entity = await GetDocumentAsync(EFChannelTemplateEntity.CreateId<T>(appId, code), ct);

            return (entity?.ToChannelTemplate<T>(jsonOptions), entity?.Etag);
        }
    }

    public async Task UpsertAsync(ChannelTemplate<T> template, string? oldEtag = null,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFChannelTemplateRepository/UpsertAsync"))
        {
            await UpsertDocumentAsync(EFChannelTemplateEntity.FromChannelTemplate(template, jsonOptions), oldEtag, ct);
        }
    }

    public async Task DeleteAsync(string appId, string id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFChannelTemplateRepository/DeleteAsync"))
        {
            await DeleteDocumentAsync(EFChannelTemplateEntity.CreateId<T>(appId, id), ct);
        }
    }

    protected override void BuildUpdate(UpdateSettersBuilder<EFChannelTemplateEntity> update, EFChannelTemplateEntity entity)
    {
        update.SetProperty(x => x.Name, entity.Name);
        update.SetProperty(x => x.Primary, entity.Primary);
    }
}
