// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Events;
using Notifo.Domain.UserEvents;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;

namespace Notifo.Domain.Counters
{
    public struct CounterKey
    {
        public Guid NotificationId { get; private set; }

        public string? EventId { get; private set; }

        public string? AppId { get; private set; }

        public string? UserId { get; private set; }

        public string? Topic { get; private set; }

        public static CounterKey ForEvent(EventMessage @event)
        {
            Guard.NotNull(@event, nameof(@event));

            var result = default(CounterKey);

            result.EventId = @event.Id;
            result.AppId = @event.AppId;
            result.Topic = @event.Topic;

            return result;
        }

        public static CounterKey ForUserEvent(UserEventMessage @event)
        {
            Guard.NotNull(@event, nameof(@event));

            var result = default(CounterKey);

            result.EventId = @event.EventId;
            result.AppId = @event.AppId;
            result.Topic = @event.Topic;
            result.UserId = @event.UserId;

            return result;
        }

        public static CounterKey ForNotification(IUserNotification notification)
        {
            Guard.NotNull(notification, nameof(notification));

            var result = default(CounterKey);

            result.NotificationId = notification.Id;
            result.EventId = notification.EventId;
            result.AppId = notification.AppId;
            result.Topic = notification.Topic;
            result.UserId = notification.UserId;

            return result;
        }
    }
}
