// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Domain.Apps;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos
{
    public sealed class CreateEmailTemplateDto
    {
        /// <summary>
        /// The new language.
        /// </summary>
        [Required]
        public string Language { get; set; }

        public CreateAppEmailTemplate ToUpdate()
        {
            return new CreateAppEmailTemplate
            {
                Language = Language
            };
        }
    }
}
