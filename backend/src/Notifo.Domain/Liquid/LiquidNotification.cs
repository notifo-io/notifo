// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;

namespace Notifo.Domain.Liquid;

public sealed class LiquidNotification : LiquidNotificationBase
{
    private readonly BaseUserNotification notification;
    private readonly Guid configurationId;
    private readonly string channel;
    private readonly string imagePresetSmall;
    private readonly string imagePresetLarge;
    private readonly IImageFormatter imageFormatter;
    private string? confirmUrl;
    private string? trackDeliveredUrl;
    private string? trackSeenUrl;
    private LiquidChildNotification[]? children;

    public string? ConfirmText
    {
        get => notification.Formatting.ConfirmText.OrNull();
    }

    public string? TrackSeenUrl
    {
        get => trackSeenUrl ??= notification.ComputeTrackSeenUrl(channel, configurationId);
    }

    public string? TrackDeliveredUrl
    {
        get => trackDeliveredUrl ??= notification.ComputeTrackDeliveredUrl(channel, configurationId);
    }

    public string? ConfirmUrl
    {
        get => confirmUrl ??= notification.ComputeConfirmUrl(channel, configurationId);
    }

    public LiquidChildNotification[] Children
    {
        get => children
            ??= notification.ChildNotifications?.Select(x =>
                new LiquidChildNotification(
                    x,
                    imagePresetSmall,
                    imagePresetLarge,
                    imageFormatter)).ToArray()
            ?? Array.Empty<LiquidChildNotification>();
    }

    public LiquidNotification(
        BaseUserNotification notification,
        Guid configurationId,
        string channel,
        string imagePresetSmall,
        string imagePresetLarge,
        IImageFormatter imageFormatter)
        : base(notification.Formatting, imagePresetSmall, imagePresetLarge, imageFormatter)
    {
        this.notification = notification;
        this.channel = channel;
        this.imagePresetSmall = imagePresetSmall;
        this.imagePresetLarge = imagePresetLarge;
        this.imageFormatter = imageFormatter;
        this.configurationId = configurationId;
    }
}
