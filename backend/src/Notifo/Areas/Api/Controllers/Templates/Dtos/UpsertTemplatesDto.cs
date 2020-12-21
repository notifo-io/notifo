// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Areas.Api.Controllers.Templates.Dtos
{
    public sealed class UpsertTemplatesDto
    {
        /// <summary>
        /// The templates to update.
        /// </summary>
        public UpsertTemplateDto[] Requests { get; set; }
    }
}
