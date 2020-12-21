// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Templates;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Templates.Dtos
{
    public sealed class TemplateDto
    {
        /// <summary>
        /// The code of the template.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The formatting.
        /// </summary>
        public NotificationFormattingDto Formatting { get; set; }

        /// <summary>
        /// Notification settings per channel.
        /// </summary>
        public NotificationSettingsDto Settings { get; set; }

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
