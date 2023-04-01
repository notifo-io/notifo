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

public sealed class ChannelTemplateDetailsDto<T>
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
    /// The date time (ISO 8601) when the template has been created.
    /// </summary>
    public Instant Created { get; set; }

    /// <summary>
    /// The date time (ISO 8601) when the template has been updated.
    /// </summary>
    public Instant LastUpdate { get; set; }

    /// <summary>
    /// The language specific templates.
    /// </summary>
    public Dictionary<string, T> Languages { get; set; }

    public static ChannelTemplateDetailsDto<T> FromDomainObject<TInput>(ChannelTemplate<TInput> source, Func<TInput, T> factory)
    {
        var result = SimpleMapper.Map(source, new ChannelTemplateDetailsDto<T>());

        result.Languages = new Dictionary<string, T>();

        if (source.Languages != null)
        {
            foreach (var (key, value) in source.Languages)
            {
                result.Languages[key] = factory(value);
            }
        }

        return result;
    }
}
