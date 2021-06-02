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

        public ParsedTemplate? ParsedBodyText { get; set; }

        public ParsedTemplate? ParsedBodyHtml { get; set; }
    }
}
