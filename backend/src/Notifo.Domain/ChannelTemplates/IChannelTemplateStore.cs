// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain.ChannelTemplates;

public interface IChannelTemplateStore<T> where T : class
{
    Task<IResultList<ChannelTemplate<T>>> QueryAsync(string appId, ChannelTemplateQuery query,
        CancellationToken ct = default);

    Task<(TemplateResolveStatus Status, T?)> GetBestAsync(string appId, string? name, string language,
        CancellationToken ct = default);

    Task<ChannelTemplate<T>?> GetAsync(string appId, string id,
        CancellationToken ct = default);

    Task<ChannelTemplate<T>> UpsertAsync(string appId, string? id, ICommand<ChannelTemplate<T>> update,
        CancellationToken ct = default);

    Task DeleteAsync(string appId, string id,
        CancellationToken ct = default);
}
