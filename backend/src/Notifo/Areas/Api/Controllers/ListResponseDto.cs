// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Notifo.Areas.Api.Controllers
{
    public class ListResponseDto<T>
    {
        /// <summary>
        /// The items.
        /// </summary>
        [Required]
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// The total number of items.
        /// </summary>
        [Required]
        public long Total { get; set; }
    }
}
