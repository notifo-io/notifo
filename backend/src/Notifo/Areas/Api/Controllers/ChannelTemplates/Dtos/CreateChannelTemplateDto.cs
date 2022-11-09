// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.ChannelTemplates;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;

public sealed class CreateChannelTemplateDto
{
    /// <summary>
    /// The kind of the template.
    /// </summary>
    public string? Kind { get; set; }

    public CreateChannelTemplate<T> ToUpdate<T>(string language)
    {
        var result = SimpleMapper.Map(this, new CreateChannelTemplate<T>());

        result.Language = language;

        return result;
    }
}
