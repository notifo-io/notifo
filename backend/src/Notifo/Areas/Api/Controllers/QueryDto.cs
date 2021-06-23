// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers
{
    public class QueryDto
    {
        /// <summary>
        /// The optional query to search for items.
        /// </summary>
        [FromQuery(Name = "query")]
        public string? Query { get; set; }

        /// <summary>
        /// The number of items to return.
        /// </summary>
        [FromQuery(Name = "take")]
        public int Take { get; set; } = 20;

        /// <summary>
        /// The number of items to skip.
        /// </summary>
        [FromQuery(Name = "skip")]
        public int Skip { get; set; }

        public T ToQuery<T>(bool needsTotal) where T : QueryBase, new()
        {
            var result = SimpleMapper.Map(this, new T
            {
                TotalNeeded = needsTotal
            });

            return result;
        }
    }
}
