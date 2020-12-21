// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Notifo.Areas.Api.Controllers
{
    public class ListResponseDto<T>
    {
        /// <summary>
        /// The items.
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// The total number of items.
        /// </summary>
        public long Total { get; set; }
    }
}
