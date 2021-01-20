// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Notifo.Domain.Templates;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Templates.Dtos
{
    public sealed class TemplateDto
    {
        /// <summary>
        /// The code of the template.
        /// </summary>
        [Required]
        public string Code { get; set; }

        /// <summary>
        /// The formatting.
        /// </summary>
        [Required]
        public NotificationFormattingDto Formatting { get; set; }

        /// <summary>
        /// Notification settings per channel.
        /// </summary>
        [Required]
        public Dictionary<string, NotificationSettingDto> Settings { get; set; }

        public static TemplateDto FromDomainObject(Template template)
        {
            var result = SimpleMapper.Map(template, new TemplateDto());

            if (template.Formatting != null)
            {
                result.Formatting = NotificationFormattingDto.FromDomainObject(template.Formatting);
            }
            else
            {
                result.Formatting = new NotificationFormattingDto();
            }

            result.Settings ??= new Dictionary<string, NotificationSettingDto>();

            if (template.Settings != null)
            {
                foreach (var (key, value) in template.Settings)
                {
                    if (value != null)
                    {
                        result.Settings[key] = NotificationSettingDto.FromDomainObject(value);
                    }
                }
            }

            return result;
        }
    }
}
