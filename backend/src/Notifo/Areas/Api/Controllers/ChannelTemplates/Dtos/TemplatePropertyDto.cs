// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Liquid;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;

public sealed class TemplatePropertyDto
{
    /// <summary>
    /// The property path.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// The data ty.
    /// </summary>
    public LiquidPropertyType Type { get; set; }

    /// <summary>
    /// The optional description.
    /// </summary>
    public string? Description { get; set; }

    public static TemplatePropertyDto FromDomainObject(LiquidProperty source)
    {
        return SimpleMapper.Map(source, new TemplatePropertyDto());
    }
}
