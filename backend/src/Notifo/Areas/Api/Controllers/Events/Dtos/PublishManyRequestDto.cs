// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Areas.Api.Controllers.Events.Dtos
{
    public sealed class PublishManyRequestDto
    {
        /// <summary>
        /// The publish requests.
        /// </summary>
        public PublishRequestDto[] Requests { get; set; }
    }
}
