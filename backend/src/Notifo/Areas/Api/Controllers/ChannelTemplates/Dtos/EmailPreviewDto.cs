// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
    public EmailPreviewErrorDto[]? Errors { get; set; }
}
