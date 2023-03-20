// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.MobilePush;

public sealed class MobilePushJob : ChannelJob
{
    public string DeviceToken { get; init; }

    public string? DeviceIdentifier { get; init; }

    public MobileDeviceType DeviceType { get; init; }

    public string ScheduleKey
    {
        get => string.Join("_",
            Notification.AppId,
            Notification.UserId,
            GroupKey.OrDefault(Notification.Id),
            DeviceToken);
    }

    public MobilePushJob()
    {
    }

    public MobilePushJob(UserNotification notification, ChannelContext context, MobilePushToken token)
        : base(notification, context)
    {
        DeviceIdentifier = token.DeviceIdentifier;
        DeviceToken = token.Token;
        DeviceType = token.DeviceType;
        Notification = notification;
    }
}
