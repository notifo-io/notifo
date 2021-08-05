// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;

namespace Notifo.Domain.Topics
{
    public interface ITopicRepository : ICounterStore<(string AppId, string Path)>
    {
        Task<IResultList<Topic>> QueryAsync(string appId, TopicQuery query,
            CancellationToken ct);
    }
}
