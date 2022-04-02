// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using NodaTime;
using Notifo.Domain.Topics;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Reflection;
using Notifo.Infrastructure.Texts;

namespace Notifo.Areas.Api.Controllers.Topics.Dtos
{
    public sealed class TopicDto
    {
        private static readonly Dictionary<string, long> EmptyCounters = new Dictionary<string, long>();

        /// <summary>
        /// The topic path.
        /// </summary>
        [Required]
        public string Path { get; set; }

        /// <summary>
        /// The date time (ISO 8601) when the topic has been created.
        /// </summary>
        [Required]
        public Instant Created { get; set; }

        /// <summary>
        /// The date time (ISO 8601) when the topic has been updated.
        /// </summary>
        [Required]
        public Instant LastUpdate { get; set; }

        /// <summary>
        /// True when the topic is explicit.
        /// </summary>
        public bool IsExplicit { get; init; }

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
        public bool ShowAutomatically { get; set; }

        /// <summary>
        /// The channel settings.
        /// </summary>
        public ReadonlyDictionary<string, TopicChannel>? Channels { get; set; }

        /// <summary>
        /// The statistics counters.
        /// </summary>
        [Required]
        public Dictionary<string, long> Counters { get; set; }

        public static TopicDto FromDomainObject(Topic source)
        {
            var result = SimpleMapper.Map(source, new TopicDto());

            result.Counters = source.Counters ?? EmptyCounters;

            return result;
        }
    }
}
