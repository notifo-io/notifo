// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos;

public sealed class IntegrationPropertyDto
{
    /// <summary>
    /// The field name for the property.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The editor type.
    /// </summary>
    public PropertyType Type { get; set; }

    /// <summary>
    /// The optional description.
    /// </summary>
    public string? EditorDescription { get; set; }

    /// <summary>
    /// The optional label.
    /// </summary>
    public string? EditorLabel { get; set; }

    /// <summary>
    /// True to show this property in the summary.
    /// </summary>
    public bool Summary { get; set; }

    /// <summary>
    /// The allowed values.
    /// </summary>
    public string[]? AllowedValues { get; init; }

    /// <summary>
    /// True when required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// The min value (for numbers).
    /// </summary>
    public long? MinValue { get; set; }

    /// <summary>
    /// The max value (for numbers).
    /// </summary>
    public long? MaxValue { get; set; }

    /// <summary>
    /// The min length (for strings).
    /// </summary>
    public long? MinLength { get; set; }

    /// <summary>
    /// The min length (for strings).
    /// </summary>
    public long? MaxLength { get; set; }

    /// <summary>
    /// The pattern (for strings).
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    /// The default value.
    /// </summary>
    public object? DefaultValue { get; set; }

    public static IntegrationPropertyDto FromDomainObject(IntegrationProperty source)
    {
        var result = SimpleMapper.Map(source, new IntegrationPropertyDto());

        return result;
    }
}
