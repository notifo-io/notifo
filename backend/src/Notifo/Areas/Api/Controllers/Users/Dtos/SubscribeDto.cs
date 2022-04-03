// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Domain;
using Notifo.Domain.Subscriptions;

namespace Notifo.Areas.Api.Controllers.Users.Dtos
{
    public sealed class SubscribeDto
    {
        /// <summary>
        /// The topic to add.
        /// </summary>
        [Required]
        public string TopicPrefix { get; set; }

        /// <summary>
        /// Notification settings per channel.
        /// </summary>
        public Dictionary<string, NotificationSettingDto>? TopicSettings { get; set; }

        public Subscribe ToUpdate()
        {
            var result = new Subscribe();

            if (TopicSettings?.Any() == true)
            {
                result.TopicSettings = new NotificationSettings();

                foreach (var (key, value) in TopicSettings)
                {
                    if (value != null)
                    {
                        result.TopicSettings[key] = value.ToDomainObject();
                    }
                }
            }

            return result;
        }
    }
}
