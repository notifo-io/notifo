// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels.Email
{
    public sealed class EmailMessage
    {
        public string FromEmail { get; set; }

        public string? FromName { get; set; }

        public string RecipientEmail { get; set; }

        public string? RecipientName { get; set; }

        public string Subject { get; set; }

        public string? BodyText { get; set; }

        public string? BodyHtml { get; set; }
    }
}
