// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain.Templates;

public interface ITemplateRepository
{
    Task<IResultList<Template>> QueryAsync(string appId, TemplateQuery query,
        CancellationToken ct = default);

    Task<(Template? Template, string? Etag)> GetAsync(string appId, string code,
        CancellationToken ct = default);

    Task UpsertAsync(Template template, string? oldEtag = null,
        CancellationToken ct = default);

    Task DeleteAsync(string appId, string code,
        CancellationToken ct = default);
}
