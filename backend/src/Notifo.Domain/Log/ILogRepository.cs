// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Infrastructure;

namespace Notifo.Domain.Log;

public interface ILogRepository
{
    Task<IResultList<LogEntry>> QueryAsync(string appId, LogQuery query,
        CancellationToken ct = default);

    Task<IResultList<LogEntry>> BatchWriteAsync(IEnumerable<(LogWrite Write, int Count, Instant Now)> updates,
        CancellationToken ct = default);
}
