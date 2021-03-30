// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Notifo.Infrastructure;

namespace Notifo.Domain.Log
{
    public interface ILogRepository
    {
        Task<IResultList<LogEntry>> QueryAsync(string appId, LogQuery query, CancellationToken ct);

        Task MatchWriteAsync(IEnumerable<(string AppId, string Message, int Count)> updates, Instant now, CancellationToken ct);
    }
}
