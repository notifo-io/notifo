// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Areas.Api.OpenApi;
using Notifo.Domain.Apps;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos;

[OpenApiRequest]
public sealed class AddContributorDto
{
    /// <summary>
    /// The email of the new contributor.
    /// </summary>
    [Required]
    public string Email { get; set; }

    /// <summary>
    /// The role.
    /// </summary>
    public string? Role { get; set; }

    public AddContributor ToUpdate()
    {
        var result = new AddContributor
        {
            EmailOrId = Email
        };

        if (Role != null)
        {
            result.Role = Role;
        }

        return result;
    }
}
