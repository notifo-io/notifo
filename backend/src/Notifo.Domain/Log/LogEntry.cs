// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Domain.Log;

public sealed class LogEntry
{
    public string AppId { get; init; }

    public string Message { get; init; }

    public string? System { get; init; }

    public string FirstWriteId { get; init; }

    public Instant FirstSeen { get; init; }

    public Instant LastSeen { get; init; }

    public int EventCode { get; init; }

    public long Count { get; init; }
}
