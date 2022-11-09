// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using NodaTime;
using Notifo.Domain.ChannelTemplates;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;

public sealed class ChannelTemplateDto
{
    /// <summary>
    /// The id of the template.
    /// </summary>
    [Required]
    public string Id { get; set; }

    /// <summary>
    /// The optional name of the template.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The kind of the template.
    /// </summary>
    public string? Kind { get; set; }

    /// <summary>
    /// True, when the template is the primary template.
    /// </summary>
    [Required]
    public bool Primary { get; set; }

    /// <summary>
    /// The last time the template has been updated.
    /// </summary>
    [Required]
    public Instant LastUpdate { get; set; }

    public static ChannelTemplateDto FromDomainObject<T>(ChannelTemplate<T> source)
    {
        var result = SimpleMapper.Map(source, new ChannelTemplateDto());

        return result;
    }
}
