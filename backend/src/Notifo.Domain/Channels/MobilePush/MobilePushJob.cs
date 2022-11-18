// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.MobilePush;

public sealed class MobilePushJob : ChannelJob
{
    public BaseUserNotification Notification { get; init; }

    public string Token { get; init; }

    public string? DeviceIdentifier { get; init; }

    public MobileDeviceType DeviceType { get; init; }

    public bool IsConfirmed { get; init; }

    public string ScheduleKey
    {
        get => $"{Notification.Id}_{Token}";
    }

    public MobilePushJob()
    {
    }

    public MobilePushJob(UserNotification notification, ChannelSetting? setting, Guid configurationId, MobilePushToken token, bool isUpdate)
        : base(notification, setting, configurationId, isUpdate, Providers.MobilePush)
    {
        Token = token.Token;
        DeviceIdentifier = token.DeviceIdentifier;
        DeviceType = token.DeviceType;
        IsConfirmed = notification.FirstConfirmed != null;
        Notification = notification;
    }
}
