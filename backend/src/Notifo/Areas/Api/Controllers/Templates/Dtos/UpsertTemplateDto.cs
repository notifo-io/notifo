// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Domain;
using Notifo.Domain.Templates;
using Notifo.Infrastructure.Reflection;
using Notifo.Infrastructure.Texts;

namespace Notifo.Areas.Api.Controllers.Templates.Dtos
{
    public sealed class UpsertTemplateDto
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
        public Dictionary<string, ChannelSettingDto>? Settings { get; set; }

        public UpsertTemplate ToUpdate()
        {
            var result = SimpleMapper.Map(this, new UpsertTemplate());

            if (Formatting != null)
            {
                result.Formatting = Formatting.ToDomainObject();
            }
            else
            {
                result.Formatting = new NotificationFormatting<LocalizedText>();
            }

            if (Settings != null)
            {
                result.Settings = new ChannelSettings();

                foreach (var (key, value) in Settings)
                {
                    if (value != null)
                    {
                        result.Settings[key] = value.ToDomainObject();
                    }
                }
            }

            return result;
        }
    }
}
