// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.Email;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;

public sealed class EmailPreviewDto
{
    /// <summary>
    /// The rendered preview.
    /// </summary>
    public string? Result { get; set; }

    /// <summary>
    /// The errors when rendering a preview.
    /// </summary>
    public EmailFormattingError[]? Errors { get; set; }
}
