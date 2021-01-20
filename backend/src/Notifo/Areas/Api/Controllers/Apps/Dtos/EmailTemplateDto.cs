// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels.Email;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos
{
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

        public static EmailTemplateDto FromDomainObject(EmailTemplate source)
        {
            var result = SimpleMapper.Map(source, new EmailTemplateDto());

            return result;
        }

        public UpdateAppEmailTemplate ToUpdate(string language)
        {
            var result = SimpleMapper.Map(this, new EmailTemplate());

            return new UpdateAppEmailTemplate { EmailTemplate = result, Language = language };
        }
    }
}
