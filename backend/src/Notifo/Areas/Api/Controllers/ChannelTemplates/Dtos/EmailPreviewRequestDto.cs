// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Domain.Channels.Email;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos
{
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

        /// <summary>
        /// The kind of the template.
        /// </summary>
        public string? Kind { get; set; }

        public EmailTemplate ToEmailTemplate()
        {
            var emailTemplate = new EmailTemplate
            {
                Kind = Kind
            };

            if (Type == EmailPreviewType.Html)
            {
                emailTemplate.BodyHtml = Template;
            }
            else
            {
                emailTemplate.BodyText = Template;
            }

            return emailTemplate;
        }
    }
}
