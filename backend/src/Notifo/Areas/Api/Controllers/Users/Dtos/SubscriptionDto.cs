// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Domain.Subscriptions;

namespace Notifo.Areas.Api.Controllers.Users.Dtos
{
    public sealed class SubscriptionDto
    {
        /// <summary>
        /// The topic to add.
        /// </summary>
        [Required]
        public string TopicPrefix { get; set; }

        /// <summary>
        /// Notification settings per channel.
        /// </summary>
        [Required]
        public Dictionary<string, NotificationSettingDto> TopicSettings { get; set; } = new Dictionary<string, NotificationSettingDto>();

        public static SubscriptionDto FromDomainObject(Subscription subscription)
        {
            var result = new SubscriptionDto
            {
                TopicPrefix = subscription.TopicPrefix
            };

            if (subscription.TopicSettings != null)
            {
                foreach (var (key, value) in subscription.TopicSettings)
                {
                    if (value != null)
                    {
                        result.TopicSettings[key] = NotificationSettingDto.FromDomainObject(value);
                    }
                }
            }

            return result;
        }
    }
}
