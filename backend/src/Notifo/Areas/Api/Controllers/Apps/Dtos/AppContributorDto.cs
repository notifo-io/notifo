// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos
{
    public sealed class AppContributorDto
    {
        /// <summary>
        /// The id of the user.
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// The name of the user.
        /// </summary>
        [Required]
        public string UserName { get; set; }

        /// <summary>
        /// The role.
        /// </summary>
        [Required]
        public string Role { get; set; }
    }
}
