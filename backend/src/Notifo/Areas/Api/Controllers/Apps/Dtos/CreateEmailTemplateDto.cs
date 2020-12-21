// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos
{
    public sealed class CreateEmailTemplateDto
    {
        /// <summary>
        /// The new language.
        /// </summary>
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
