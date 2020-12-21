// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Subscriptions
{
    public sealed class Subscription
    {
        public string AppId { get; set; }

        public string UserId { get; set;  }

        public TopicId TopicPrefix { get; set; }

        public NotificationSettings TopicSettings { get; set; }
    }
}
