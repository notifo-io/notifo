// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Areas.Api.OpenApi;
using System.ComponentModel.DataAnnotations;

namespace Notifo.Areas.Api.Controllers.Templates.Dtos;

[OpenApiRequest]
public sealed class UpsertTemplatesDto
{
    /// <summary>
    /// The templates to update.
    /// </summary>
    [Required]
    public UpsertTemplateDto[] Requests { get; set; }
}
