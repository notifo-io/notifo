// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure;

namespace Notifo.Domain.ChannelTemplates
{
    public interface IChannelTemplateRepository<T> where T : class
    {
        Task<IResultList<ChannelTemplate<T>>> QueryAsync(string appId, ChannelTemplateQuery query,
            CancellationToken ct = default);

        Task<ChannelTemplate<T>?> GetBestAsync(string appId, string? name,
            CancellationToken ct = default);

        Task<(ChannelTemplate<T>? Template, string? Etag)> GetAsync(string appId, string code,
            CancellationToken ct = default);

        Task UpsertAsync(ChannelTemplate<T> template, string? oldEtag = null,
            CancellationToken ct = default);

        Task DeleteAsync(string appId, string code,
            CancellationToken ct = default);
    }
}
