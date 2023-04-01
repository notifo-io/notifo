// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Areas.Api.OpenApi;

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
