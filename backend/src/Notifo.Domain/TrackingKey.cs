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

namespace Notifo.Domain;

public sealed record TrackingKey
{
    required public Guid UserNotificationId { get; init; }

    required public string? EventId { get; init; }

    required public string? AppId { get; init; }

    required public string? UserId { get; init; }

    required public string? Topic { get; init; }

    required public string? Channel { get; init; }

    required public Guid ConfigurationId { get; init; }

    public TrackingToken ToToken()
    {
        return new TrackingToken(UserNotificationId, Channel, ConfigurationId);
    }

    public static TrackingKey ForEvent(EventMessage @event)
    {
        Guard.NotNull(@event, nameof(@event));

        return new TrackingKey
        {
            AppId = @event.AppId,
            Channel = null,
            ConfigurationId = default,
            EventId = @event.Id,
            UserNotificationId = default,
            UserId = null,
            Topic = @event.Topic,
        };
    }

    public static TrackingKey ForUserEvent(UserEventMessage @event)
    {
        Guard.NotNull(@event, nameof(@event));

        return new TrackingKey
        {
            AppId = @event.AppId,
            Channel = null,
            ConfigurationId = default,
            EventId = @event.EventId,
            UserNotificationId = default,
            UserId = @event.UserId,
            Topic = @event.Topic,
        };
    }

    public static TrackingKey ForNotification(BaseUserNotification notification, string? channel = null, Guid configurationId = default)
    {
        Guard.NotNull(notification);

        return new TrackingKey
        {
            AppId = notification.AppId,
            Channel = channel,
            ConfigurationId = configurationId,
            EventId = notification.EventId,
            UserNotificationId = notification.Id,
            UserId = notification.UserId,
            Topic = notification.Topic,
        };
    }
}
