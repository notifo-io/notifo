// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Areas.Api.Controllers.Events.Dtos
{
    public sealed class PublishManyDto
    {
        /// <summary>
        /// The publish requests.
        /// </summary>
        public PublishDto[] Requests { get; set; }
    }
}
