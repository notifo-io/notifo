// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Subscriptions
{
    public sealed record Subscription
    {
        public string AppId { get; init; }

        public string UserId { get; init;  }

        public TopicId TopicPrefix { get; init; }

        public NotificationSettings? TopicSettings { get; init; }

        public static Subscription Create(string appId, string userId, TopicId prefix)
        {
            var subscription = new Subscription
            {
                AppId = appId,
                TopicPrefix = prefix,
                TopicSettings = null,
                UserId = userId,
            };

            return subscription;
        }
    }
}
