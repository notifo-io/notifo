// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Notifo.Domain.Events;
using Notifo.Domain.UserEvents;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;

namespace Notifo.Domain.Counters
{
    public struct CounterKey
    {
        private Guid notificationId;
        private string? eventId;
        private string? appId;
        private string? userId;
        private string? topic;

        public Guid NotificationId => notificationId;

        public string? EventId => eventId;

        public string? AppId => appId;

        public string? UserId => userId;

        public string? Topic => topic;

        public static CounterKey ForEvent(EventMessage @event)
        {
            Guard.NotNull(@event, nameof(@event));

            var result = default(CounterKey);

            result.eventId = @event.Id;
            result.appId = @event.AppId;
            result.topic = @event.Topic;

            return result;
        }

        public static CounterKey ForUserEvent(UserEventMessage @event)
        {
            Guard.NotNull(@event, nameof(@event));

            var result = default(CounterKey);

            result.eventId = @event.EventId;
            result.appId = @event.AppId;
            result.topic = @event.Topic;
            result.userId = @event.UserId;

            return result;
        }

        public static CounterKey ForNotification(IUserNotification notification)
        {
            Guard.NotNull(notification, nameof(notification));

            var result = default(CounterKey);

            result.notificationId = notification.Id;
            result.eventId = notification.EventId;
            result.appId = notification.AppId;
            result.topic = notification.Topic;
            result.userId = notification.UserId;

            return result;
        }
    }
}
