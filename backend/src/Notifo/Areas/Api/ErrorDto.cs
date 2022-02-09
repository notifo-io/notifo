// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Areas.Api
{
    public sealed class ErrorDto
    {
        /// <summary>
        /// The error message.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// The error code.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// The optional trace id.
        /// </summary>
        public string? TraceId { get; set; }

        /// <summary>
        /// The error type, usually a link.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Detailed error messages.
        /// </summary>
        public string[]? Details { get; set; }

        /// <summary>
        /// Status code of the http response.
        /// </summary>
        public int StatusCode { get; set; } = 400;
    }
}
