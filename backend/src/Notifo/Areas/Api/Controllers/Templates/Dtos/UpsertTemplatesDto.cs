// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Notifo.Areas.Api.Controllers.Templates.Dtos
{
    public sealed class UpsertTemplatesDto
    {
        /// <summary>
        /// The templates to update.
        /// </summary>
        [Required]
        public UpsertTemplateDto[] Requests { get; set; }
    }
}
