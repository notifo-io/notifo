// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Log;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Logs.Dtos
{
    public sealed class LogEntryDto
    {
        /// <summary>
        /// The log message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The first time this message has been seen.
        /// </summary>
        public Instant FirstSeen { get; set; }

        /// <summary>
        /// The last time this message has been seen.
        /// </summary>
        public Instant LastSeen { get; set; }

        /// <summary>
        /// The number of items the message has been seen.
        /// </summary>
        public long Count { get; set; }

        public static LogEntryDto FromDomainObject(LogEntry source)
        {
            var result = SimpleMapper.Map(source, new LogEntryDto());

            return result;
        }
    }
}
