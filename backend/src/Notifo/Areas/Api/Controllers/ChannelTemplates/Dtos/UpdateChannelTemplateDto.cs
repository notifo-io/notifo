// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Areas.Api.OpenApi;
using Notifo.Domain.ChannelTemplates;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;

[OpenApiRequest]
public class UpdateChannelTemplateDto<TDto>
{
    /// <summary>
    /// The name of the template.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// True, when the template is the primary template.
    /// </summary>
    public bool? Primary { get; set; }

    /// <summary>
    /// The language specific templates.
    /// </summary>
    public Dictionary<string, TDto>? Languages { get; set; }

    public UpdateChannelTemplate<T> ToUpdate<T>(string code, Func<TDto, T> mapper)
    {
        var result = SimpleMapper.Map(this, new UpdateChannelTemplate<T>());

        result.TemplateCode = code;
        result.Languages = Languages?.ToDictionary(x => x.Key, x => mapper(x.Value));

        return result;
    }
}
