// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Counters;
using Notifo.Infrastructure;

namespace Notifo.Domain.Topics
{
    public interface ITopicRepository : ICounterStore<(string AppId, string Path)>
    {
        Task<IResultList<Topic>> QueryAsync(string appId, TopicQuery query,
            CancellationToken ct = default);

        Task<(Topic? Topic, string? Etag)> GetAsync(string appId, TopicId path,
            CancellationToken ct = default);

        Task UpsertAsync(Topic topic, string? oldEtag = null,
            CancellationToken ct = default);

        Task DeleteAsync(string appId, TopicId path,
            CancellationToken ct = default);
    }
}
