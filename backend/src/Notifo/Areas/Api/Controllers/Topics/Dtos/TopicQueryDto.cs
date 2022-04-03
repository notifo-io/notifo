// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Topics;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Topics.Dtos
{
    public sealed class TopicQueryDto : QueryDto
    {
        /// <summary>
        /// The scope of the query.
        /// </summary>
        public TopicQueryScope Scope { get; set; }

        public TopicQuery ToQuery(bool needsTotal)
        {
            var result = SimpleMapper.Map(this, new TopicQuery
            {
                TotalNeeded = needsTotal
            });

            return result;
        }
    }
}
