// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Areas.Api.OpenApi;
using System.ComponentModel.DataAnnotations;

namespace Notifo.Areas.Api.Controllers.Topics.Dtos;

[OpenApiRequest]
public sealed class UpsertTopicsDto
{
    /// <summary>
    /// The topics to update.
    /// </summary>
    [Required]
    public UpsertTopicDto[] Requests { get; set; }
}
