// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Areas.Api.OpenApi;
using System.ComponentModel.DataAnnotations;

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
