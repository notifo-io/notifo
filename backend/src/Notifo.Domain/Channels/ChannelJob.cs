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

    public TrackingKey Tracking { get; init; }

    protected ChannelJob()
    {
    }

    protected ChannelJob(BaseUserNotification notification, ChannelSetting? setting, Guid configurationId, bool isUpdate, string channel)
    {
        Delay = Duration.FromSeconds(setting?.DelayInSeconds ?? 0);
        Condition = setting?.Condition ?? ChannelCondition.Always;
        ConfigurationId = configurationId;
        IsUpdate = isUpdate;
        Tracking = TrackingKey.ForNotification(notification, channel, configurationId);
    }
}
