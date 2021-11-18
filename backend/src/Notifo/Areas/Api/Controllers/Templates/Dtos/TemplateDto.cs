// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

        public static TemplateDto FromDomainObject(Template source)
        {
            var result = SimpleMapper.Map(source, new TemplateDto());

            if (source.Formatting != null)
            {
                result.Formatting = NotificationFormattingDto.FromDomainObject(source.Formatting);
            }
            else
            {
                result.Formatting = new NotificationFormattingDto();
            }

            result.Settings ??= new Dictionary<string, NotificationSettingDto>();

            if (source.Settings != null)
            {
                foreach (var (key, value) in source.Settings)
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
