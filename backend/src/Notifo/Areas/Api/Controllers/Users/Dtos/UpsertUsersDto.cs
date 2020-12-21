// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Areas.Api.Controllers.Users.Dtos
{
    public sealed class UpsertUsersDto
    {
        /// <summary>
        /// The users to update.
        /// </summary>
        public UpsertUserDto[] Requests { get; set; }
    }
}
