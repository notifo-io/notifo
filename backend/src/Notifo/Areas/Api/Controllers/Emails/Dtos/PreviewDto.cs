// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.Email;

namespace Notifo.Areas.Api.Controllers.Emails.Dto
{
    public sealed class PreviewDto
    {
        public string? Result { get; set; }

        public EmailFormattingError[]? Errors { get; set; }
    }
}
