// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Areas.Api.OpenApi;
using Notifo.Identity;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.SystemUsers.Dtos;

[OpenApiRequest]
public class CreateSystemUserDto
{
    /// <summary>
    /// The email of the user. Unique value.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    /// <summary>
    /// The password of the user.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Additional role for the user.
    /// </summary>
    [Required]
    public HashSet<string> Roles { get; set; }

    public UserValues ToValues()
    {
        return SimpleMapper.Map(this, new UserValues());
    }
}
