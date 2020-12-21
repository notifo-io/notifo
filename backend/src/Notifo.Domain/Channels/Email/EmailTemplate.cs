// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels.Email
{
    public sealed class EmailTemplate
    {
        public string Subject { get; set; }

        public string BodyHtml { get; set; }

        public string? BodyText { get; set; }
    }
}
