// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Templates;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Templates.Dtos;

public sealed class TemplateDto
{
    /// <summary>
    /// The code of the template.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// The date time (ISO 8601) when the template has been created.
    /// </summary>
    public Instant Created { get; set; }

    /// <summary>
    /// The date time (ISO 8601) when the template has been updated.
    /// </summary>
    public Instant LastUpdate { get; set; }

    /// <summary>
    /// The formatting.
    /// </summary>
    public NotificationFormattingDto Formatting { get; set; }

    /// <summary>
    /// Notification settings per channel.
    /// </summary>
    public Dictionary<string, ChannelSettingDto> Settings { get; set; } = new Dictionary<string, ChannelSettingDto>();

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
                    result.Settings[key] = ChannelSettingDto.FromDomainObject(value);
                }
            }
        }

        return result;
    }
}
