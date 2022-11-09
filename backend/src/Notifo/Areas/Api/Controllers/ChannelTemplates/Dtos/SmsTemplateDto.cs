// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;

public sealed class SmsTemplateDto
{
    /// <summary>
    /// The template text.
    /// </summary>
    [Required]
    public string Text { get; set; }
}
