// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain.Topics;

public interface ITopicStore
{
    Task<IResultList<Topic>> QueryAsync(string appId, TopicQuery query,
        CancellationToken ct = default);
}
