// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.ChannelTemplates;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;

public sealed class ChannelTemplateDto
{
    /// <summary>
    /// The id of the template.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The optional name of the template.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// True, when the template is the primary template.
    /// </summary>
    public bool Primary { get; set; }

    /// <summary>
    /// The last time the template has been updated.
    /// </summary>
    public Instant LastUpdate { get; set; }

    public static ChannelTemplateDto FromDomainObject<T>(ChannelTemplate<T> source)
    {
        var result = SimpleMapper.Map(source, new ChannelTemplateDto());

        return result;
    }
}
