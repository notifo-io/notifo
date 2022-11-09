// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Notifo.Areas.Api.Controllers.Topics.Dtos;

public sealed class UpsertTopicsDto
{
    /// <summary>
    /// The topics to update.
    /// </summary>
    [Required]
    public UpsertTopicDto[] Requests { get; set; }
}
