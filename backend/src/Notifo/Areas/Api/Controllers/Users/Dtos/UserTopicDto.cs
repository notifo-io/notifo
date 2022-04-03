// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Domain.Topics;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Reflection;
using Notifo.Infrastructure.Texts;

namespace Notifo.Areas.Api.Controllers.Users.Dtos
{
    public sealed class UserTopicDto
    {
        /// <summary>
        /// The path.
        /// </summary>
        [Required]
        public string Path { get; set; }

        /// <summary>
        /// The name.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The optional description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// True to show the topic automatically to new users, e.g. when he accepts push notifications.
        /// </summary>
        public bool ShowAutomatically { get; set; }

        /// <summary>
        /// The channel options.
        /// </summary>
        [Required]
        public ReadonlyDictionary<string, TopicChannel> Channels { get; set; }

        public static UserTopicDto FromDomainObject(Topic topic, string? language, string masterLanguage)
        {
            var result = SimpleMapper.Map(topic, new UserTopicDto());

            static string GetText(LocalizedText source, string? language, string masterLanguage)
            {
                if (language != null && source.TryGetValue(language, out var name))
                {
                    return name;
                }
                else if (source.TryGetValue(masterLanguage, out name))
                {
                    return name;
                }

                return source.Values.FirstOrDefault() ?? string.Empty;
            }

            result.Name = GetText(topic.Name, language, masterLanguage);

            if (topic.Description != null)
            {
                result.Description = GetText(topic.Description, language, masterLanguage);
            }

            return result;
        }
    }
}
