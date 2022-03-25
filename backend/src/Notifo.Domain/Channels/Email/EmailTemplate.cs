// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.Email.Formatting;

namespace Notifo.Domain.Channels.Email
{
    public sealed class EmailTemplate
    {
        public string Subject { get; set; }

        public string BodyHtml { get; set; }

        public string? BodyText { get; set; }

        public string? FromEmail { get; set; }

        public string? FromName { get; set; }

        public string? Kind { get; set; }

        public ParsedEmailTemplate? ParsedBodyText { get; set; }

        public ParsedEmailTemplate? ParsedBodyHtml { get; set; }
    }
}
