// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure;

namespace Notifo.Domain.Templates
{
    public interface ITemplateRepository
    {
        Task<IResultList<Template>> QueryAsync(string appId, TemplateQuery query, CancellationToken ct);

        Task<(Template? Template, string? Etag)> GetAsync(string appId, string code, CancellationToken ct);

        Task UpsertAsync(Template template, string? oldEtag, CancellationToken ct);

        Task DeleteAsync(string appId, string code, CancellationToken ct);
    }
}
