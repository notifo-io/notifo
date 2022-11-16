// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Areas.Api.Controllers.Users.Dtos;

public sealed class AdminProfileDto
{
    /// <summary>
    /// The token for the integrated app.
    /// </summary>
    public string? Token { get; set; }
}
