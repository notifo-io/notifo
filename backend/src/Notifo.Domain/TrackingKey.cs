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
    public Guid UserNotificationId { get; init; }

    public string? EventId { get; init; }

    public string? AppId { get; init; }

    public string? UserId { get; init; }

    public string? Topic { get; init; }

    public string? Channel { get; init; }

    public Guid ConfigurationId { get; init; }

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
