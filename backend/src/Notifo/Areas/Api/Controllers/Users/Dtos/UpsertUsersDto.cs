// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Areas.Api.OpenApi;

namespace Notifo.Areas.Api.Controllers.Users.Dtos;

[OpenApiRequest]
public sealed class UpsertUsersDto
{
    /// <summary>
    /// The users to update.
    /// </summary>
    [Required]
    public UpsertUserDto[] Requests { get; set; }
}
