// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Counters;
using Notifo.Domain.Topics;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Topics.Dtos
{
    public sealed class TopicDto
    {
        /// <summary>
        /// The topic path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// THe last update to the topic.
        /// </summary>
        public Instant LastUpdate { get; set; }

        /// <summary>
        /// The statistics counters.
        /// </summary>
        public CounterMap Counters { get; set; }

        public static TopicDto FromTopic(Topic source)
        {
            var result = SimpleMapper.Map(source, new TopicDto());

            return result;
        }
    }
}
