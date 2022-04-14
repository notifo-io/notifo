// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Events;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Events.Dtos
{
    public sealed class EventQueryDto : QueryDto
    {
        /// <summary>
        /// The active channels.
        /// </summary>
        public string[]? Channels { get; set; }

        public EventQuery ToQuery(bool needsTotal)
        {
            var result = SimpleMapper.Map(this, new EventQuery
            {
                TotalNeeded = needsTotal
            });

            return result;
        }
    }
}
