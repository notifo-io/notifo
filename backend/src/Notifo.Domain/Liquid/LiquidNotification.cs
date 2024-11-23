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

public sealed class LiquidNotification(
    BaseUserNotification notification,
    Guid configurationId,
    string channel,
    string imagePresetSmall,
    string imagePresetLarge,
    IImageFormatter imageFormatter)
#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
    : LiquidNotificationBase(notification.Formatting, notification.Properties, imagePresetSmall, imagePresetLarge, imageFormatter)
#pragma warning restore CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
{
    private string? confirmUrl;
    private string? trackDeliveredUrl;
    private string? trackSeenUrl;
    private LiquidChildNotification[]? children;

    public string? TrackSeenUrl
    {
        get => trackSeenUrl ??= notification.ComputeTrackSeenUrl(channel, configurationId);
    }

    public string? TrackDeliveredUrl
    {
        get => trackDeliveredUrl ??= notification.ComputeTrackDeliveredUrl(channel, configurationId);
    }

    public string? ConfirmText
    {
        get => notification.Formatting.ConfirmText.OrNull();
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
            ?? [];
    }

    public static void Describe(LiquidProperties properties)
    {
        DescribeBase(properties);

        properties.AddString("trackSeenUrl",
            "The tracking URL to mark the notification as seen.");

        properties.AddString("trackDeliveredUrl",
            "The tracking URL to mark the notification as delivered.");

        properties.AddString("confirmUrl",
            "The tracking URL to mark the notification as confirmed.");

        properties.AddString("confirmText",
            "The text for confirmation buttons.");

        properties.AddArray("children",
            "The child notifications if the notifications have been grouped together.");
    }
}
