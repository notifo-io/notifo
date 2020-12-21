// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Subscriptions;

namespace Notifo.Areas.Api.Controllers.Users.Dtos
{
    public sealed class SubscriptionDto
    {
        /// <summary>
        /// The topic to add.
        /// </summary>
        public string TopicPrefix { get; set; }

        /// <summary>
        /// Notification settings per channel.
        /// </summary>
        public NotificationSettingsDto TopicSettings { get; set; }

        public static SubscriptionDto FromDomainObject(Subscription subscription)
        {
            var result = new SubscriptionDto
            {
                TopicPrefix = subscription.TopicPrefix
            };

            if (subscription.TopicSettings != null)
            {
                result.TopicSettings = new NotificationSettingsDto();

                foreach (var (key, value) in subscription.TopicSettings)
                {
                    if (value != null)
                    {
                        result.TopicSettings[key] = NotificationSettingDto.FromDomainObject(value);
                    }
                }
            }
            else
            {
                result.TopicSettings = new NotificationSettingsDto();
            }

            return result;
        }
    }
}
