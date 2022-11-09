// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;

public sealed class EmailTemplateDto
{
    /// <summary>
    /// The subject text.
    /// </summary>
    [Required]
    public string Subject { get; set; }

    /// <summary>
    /// The body html template.
    /// </summary>
    [Required]
    public string BodyHtml { get; set; }

    /// <summary>
    /// The body text template.
    /// </summary>
    public string? BodyText { get; set; }

    /// <summary>
    /// The optional from email.
    /// </summary>
    public string? FromEmail { get; set; }

    /// <summary>
    /// The optional from name.
    /// </summary>
    public string? FromName { get; set; }

    /// <summary>
    /// The type of the template.
    /// </summary>
    public string? Kind { get; set; }
}
