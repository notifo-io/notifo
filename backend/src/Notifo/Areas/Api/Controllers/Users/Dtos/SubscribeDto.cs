// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Subscriptions;

namespace Notifo.Areas.Api.Controllers.Users.Dtos
{
    public sealed class SubscribeDto
    {
        /// <summary>
        /// The topic to add.
        /// </summary>
        public string TopicPrefix { get; set; }

        /// <summary>
        /// Notification settings per channel.
        /// </summary>
        public NotificationSettingsDto TopicSettings { get; set; }

        public SubscriptionUpdate ToUpdate(string userId)
        {
            var result = new SubscriptionUpdate
            {
                TopicPrefix = TopicPrefix
            };

            if (TopicSettings != null)
            {
                foreach (var (key, value) in TopicSettings)
                {
                    if (value != null)
                    {
                        result.TopicSettings[key] = value.ToDomainObject();
                    }
                }
            }

            result.UserId = userId;

            return result;
        }
    }
}
