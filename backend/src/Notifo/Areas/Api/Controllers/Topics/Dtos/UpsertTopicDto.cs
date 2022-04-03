// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain;
using Notifo.Domain.Topics;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Reflection;
using Notifo.Infrastructure.Texts;

namespace Notifo.Areas.Api.Controllers.Topics.Dtos
{
    public sealed class UpsertTopicDto
    {
        /// <summary>
        /// The path of the topic.
        /// </summary>
        public TopicId Path { get; set; }

        /// <summary>
        /// The name.
        /// </summary>
        public LocalizedText? Name { get; set; }

        /// <summary>
        /// The description.
        /// </summary>
        public LocalizedText? Description { get; set; }

        /// <summary>
        /// True to show the topic automatically to new users, e.g. when he accepts push notifications.
        /// </summary>
        public bool? ShowAutomatically { get; set; }

        /// <summary>
        /// Settings per channel.
        /// </summary>
        public Dictionary<string, TopicChannel>? Channels { get; set; }

        public UpsertTopic ToUpdate()
        {
            var result = SimpleMapper.Map(this, new UpsertTopic());

            result.Channels = Channels?.ToReadonlyDictionary();

            return result;
        }
    }
}
