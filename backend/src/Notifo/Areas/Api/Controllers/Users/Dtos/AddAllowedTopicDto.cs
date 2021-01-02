// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Users;

namespace Notifo.Areas.Api.Controllers.Users.Dtos
{
    public sealed class AddAllowedTopicDto
    {
        /// <summary>
        /// The topic to add.
        /// </summary>
        public string Prefix { get; set; }

        public AddUserAllowedTopic ToUpdate()
        {
            return new AddUserAllowedTopic
            {
                Prefix = Prefix
            };
        }
    }
}
