// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Domain.Log
{
    public sealed class LogEntry
    {
        public string AppId { get; set; }

        public string Message { get; set; }

        public Instant FirstSeen { get; set; }

        public Instant LastSeen { get; set; }

        public long Count { get; set; }
    }
}
