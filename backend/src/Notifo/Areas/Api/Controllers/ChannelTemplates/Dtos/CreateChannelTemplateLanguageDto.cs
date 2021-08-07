// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Domain.ChannelTemplates;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos
{
    public sealed class CreateChannelTemplateLanguageDto
    {
        /// <summary>
        /// The new language.
        /// </summary>
        [Required]
        public string Language { get; set; }

        public CreateChannelTemplate<T> ToUpdate<T>()
        {
            var result = SimpleMapper.Map(this, new CreateChannelTemplate<T>());

            return result;
        }
    }
}
