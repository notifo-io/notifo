// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels.Email
{
    public sealed class EmailPreview
    {
        public EmailMessage? Message { get; set; }

        public List<EmailFormattingError>? Errors { get; set; }
    }
}
