// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos
{
    public sealed class EmailPreviewRequestDto
    {
        /// <summary>
        /// The preview to render.
        /// </summary>
        [Required]
        public string Template { get; set; }

        /// <summary>
        /// The template type.
        /// </summary>
        public EmailPreviewType Type { get; set; }
    }
}
