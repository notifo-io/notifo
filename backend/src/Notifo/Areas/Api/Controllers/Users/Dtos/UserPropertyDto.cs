// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Users.Dtos;

public sealed class UserPropertyDto
{
    /// <summary>
    /// The field name for the property.
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// The optional description.
    /// </summary>
    public string? EditorDescription { get; set; }

    /// <summary>
    /// The optional label.
    /// </summary>
    public string? EditorLabel { get; set; }

    public static UserPropertyDto FromDomainObject(IntegrationProperty property)
    {
        var result = SimpleMapper.Map(property, new UserPropertyDto());

        return result;
    }
}
