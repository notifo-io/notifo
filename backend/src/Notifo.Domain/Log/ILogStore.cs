// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure;

namespace Notifo.Domain.Log
{
    public interface ILogStore
    {
        Task<IResultList<LogEntry>> QueryAsync(string appId, LogQuery query,
            CancellationToken ct = default);

        Task LogAsync(string appId, string system, string message);

        Task LogAsync(string appId, string message);
    }
}
