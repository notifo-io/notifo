﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.UserNotifications
{
    public record struct UserNotificationTrackingIdentifier
    {
        public Guid UserNotificationId { get; init; }

        public string EventId { get; init; }

        public string AppId { get; init; }

        public string UserId { get; init; }

        public string Topic { get; init; }

        public string? Channel { get; init; }

        public Guid ConfigurationId { get; init; }

        public static UserNotificationTrackingIdentifier ForNotification(BaseUserNotification notification, string? channel = null, Guid configurationId = default)
        {
            return new UserNotificationTrackingIdentifier
            {
                AppId = notification.AppId,
                Channel = channel,
                ConfigurationId = configurationId,
                EventId = notification.EventId,
                Topic = notification.Topic,
                UserId = notification.UserId,
                UserNotificationId = notification.Id
            };
        }
    }
}