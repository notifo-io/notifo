// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Notifo.Areas.Api.Controllers.Ping.Dtos;

public sealed class InfoDto
{
    /// <summary>
    /// The actual version.
    /// </summary>
    [Required]
    public string Version { get; set; }
}
