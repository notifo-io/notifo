// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos
{
    public sealed class AddContributorDto
    {
        /// <summary>
        /// The email of the new contributor.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The role.
        /// </summary>
        public string Role { get; set; }

        public AddContributor ToUpdate(string userId)
        {
            var result = SimpleMapper.Map(this, new AddContributor());

            result.UserId = userId;

            return result;
        }
    }
}
