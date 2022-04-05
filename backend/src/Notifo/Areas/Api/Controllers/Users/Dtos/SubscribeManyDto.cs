// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain;

namespace Notifo.Areas.Api.Controllers.Users.Dtos
{
    public sealed class SubscribeManyDto
    {
        /// <summary>
        /// A list of topics to create.
        /// </summary>
        public List<SubscribeDto>? Subscribe { get; set; }

        /// <summary>
        /// A list of topics to unsubscribe from.
        /// </summary>
        public List<TopicId>? Unsubscribe { get; set; }
    }
}
