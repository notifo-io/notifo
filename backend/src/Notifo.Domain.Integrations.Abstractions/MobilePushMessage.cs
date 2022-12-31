// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public sealed class MobilePushMessage
{
    public Guid NotificationId { get; init; }

    public bool IsConfirmed { get; init; }

    public bool Silent { get; init; }

    public bool Wakeup { get; init; }

    public int? TimeToLiveInSeconds { get; init; }

    public MobileDeviceType DeviceType { get; init; }

    public string? Body { get; init; }

    public string? ConfirmText { get; init; }

    public string? ConfirmUrl { get; init; }

    public string? Data { get; init; }

    public string DeviceToken { get; init; }

    public string? ImageLarge { get; init; }

    public string? ImageSmall { get; init; }

    public string? LinkText { get; init; }

    public string? LinkUrl { get; init; }

    public string? Subject { get; init; }

    public string? TrackDeliveredUrl { get; init; }

    public string? TrackingToken { get; init; }

    public string? TrackSeenUrl { get; init; }
}
