// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Notifo.Areas.Api.Controllers.Events.Dtos;

public sealed class PublishManyDto
{
    /// <summary>
    /// The publish requests.
    /// </summary>
    [Required]
    public PublishDto[] Requests { get; set; }
}
