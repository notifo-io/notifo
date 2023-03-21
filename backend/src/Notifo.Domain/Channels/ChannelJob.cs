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

    public Duration SendDelay { get; init; }

    public ChannelCondition SendCondition { get; init; }

    public bool IsUpdate { get; init; }

    public bool IsConfirmed { get; init; }

    public string? GroupKey { get; init; }

    public string? Template { get; init; }

    public BaseUserNotification Notification { get; init; }

    protected ChannelJob()
    {
    }

    protected ChannelJob(UserNotification notification, ChannelContext context)
    {
        ConfigurationId = context.ConfigurationId;
        GroupKey = context.Setting.GroupKey;
        IsConfirmed = notification.FirstConfirmed != null;
        IsUpdate = context.IsUpdate;
        Notification = notification;
        SendCondition = context.Setting.Condition;
        SendDelay = Duration.FromSeconds(context.Setting.DelayInSeconds ?? 0);
        Template = context.Setting.Template;
    }

    public TrackingKey AsTrackingKey(string channel)
    {
        return TrackingKey.ForNotification(Notification, channel, ConfigurationId);
    }
}
