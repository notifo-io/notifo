// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels;

public abstract class ChannelJob
{
    public Guid ConfigurationId { get; init; }

    public Duration Delay { get; init; }

    public ChannelCondition Condition { get; init; }

    public bool IsUpdate { get; init; }

    public bool IsConfirmed { get; init; }

    public BaseUserNotification Notification { get; init; }

    protected ChannelJob()
    {
    }

    protected ChannelJob(UserNotification notification, ChannelContext context)
    {
        Condition = context.Setting.Condition;
        Delay = Duration.FromSeconds(context.Setting.DelayInSeconds ?? 0);
        IsConfirmed = notification.FirstConfirmed != null;
        IsUpdate = context.IsUpdate;
        Notification = notification;
    }

    public TrackingKey AsTrackingKey(string channel)
    {
        return TrackingKey.ForNotification(Notification, channel, ConfigurationId);
    }
}
