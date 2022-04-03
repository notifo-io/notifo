// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain.Topics
{
    public interface ITopicStore
    {
        Task<IResultList<Topic>> QueryAsync(string appId, TopicQuery query,
            CancellationToken ct = default);

        Task<Topic> UpsertAsync(string appId, TopicId path, ICommand<Topic> command,
            CancellationToken ct = default);

        Task DeleteAsync(string appId, TopicId path,
            CancellationToken ct = default);
    }
}
