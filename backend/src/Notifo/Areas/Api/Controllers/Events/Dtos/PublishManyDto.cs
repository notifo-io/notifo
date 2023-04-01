// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Areas.Api.OpenApi;
using System.ComponentModel.DataAnnotations;

namespace Notifo.Areas.Api.Controllers.Events.Dtos;

[OpenApiRequest]
public sealed class PublishManyDto
{
    /// <summary>
    /// The publish requests.
    /// </summary>
    [Required]
    public PublishDto[] Requests { get; set; }
}
