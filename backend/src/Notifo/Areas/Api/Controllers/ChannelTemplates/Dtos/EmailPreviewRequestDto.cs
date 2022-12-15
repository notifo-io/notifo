// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Domain.Channels.Email;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;

public sealed class EmailPreviewRequestDto
{
    /// <summary>
    /// The preview to render.
    /// </summary>
    [Required]
    public string Template { get; set; }

    /// <summary>
    /// The template type.
    /// </summary>
    public EmailPreviewType Type { get; set; }

    public EmailTemplate ToEmailTemplate()
    {
        var emailTemplate = new EmailTemplate();

        if (Type == EmailPreviewType.Html)
        {
            emailTemplate = emailTemplate with { BodyHtml = Template };
        }
        else
        {
            emailTemplate = emailTemplate with { BodyText = Template };
        }

        return emailTemplate;
    }
}
