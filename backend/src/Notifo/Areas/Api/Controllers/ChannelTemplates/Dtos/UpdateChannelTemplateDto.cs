// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.ChannelTemplates;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos
{
    public class UpdateChannelTemplateDto
    {
        /// <summary>
        /// The name of the template.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// True, when the template is the primary template.
        /// </summary>
        public bool Primary { get; set; }

        public UpdateChannelTemplate<T> ToUpdate<T>()
        {
            var result = SimpleMapper.Map(this, new UpdateChannelTemplate<T>());

            return result;
        }
    }
}
