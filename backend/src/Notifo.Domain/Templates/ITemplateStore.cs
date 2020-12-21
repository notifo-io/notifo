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
    public interface ITemplateStore
    {
        Task<IResultList<Template>> QueryAsync(string appId, TemplateQuery query, CancellationToken ct = default);

        Task<Template?> GetAsync(string appId, string code, CancellationToken ct = default);

        Task<Template> UpsertAsync(string appId, string code, TemplateUpdate update, CancellationToken ct = default);

        Task DeleteAsync(string appId, string code, CancellationToken ct = default);
    }
}
