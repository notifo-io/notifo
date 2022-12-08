// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain.Log;

public interface ILogStore
{
    Task<IResultList<LogEntry>> QueryAsync(string appId, LogQuery query,
        CancellationToken ct = default);

    Task LogAsync(string appId, LogMessage message, bool skipDefaultLog = false);

    Task LogAsync(string appId, string userId, LogMessage message, bool skipDefaultLog = false);
}
