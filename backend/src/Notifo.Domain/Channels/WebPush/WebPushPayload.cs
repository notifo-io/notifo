// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Notifo.Domain.Integrations;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Domain.Channels.WebPush;

public sealed class WebPushPayload
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonPropertyName("cu")]
    public string? ConfirmUrl { get; set; }

    [JsonPropertyName("ct")]
    public string? ConfirmText { get; set; }

    [JsonPropertyName("is")]
    public string? ImageSmall { get; init; }

    [JsonPropertyName("il")]
    public string? ImageLarge { get; init; }

    [JsonPropertyName("lt")]
    public string? LinkText { get; init; }

    [JsonPropertyName("lu")]
    public string? LinkUrl { get; init; }

    [JsonPropertyName("td")]
    public string? TrackDeliveredUrl { get; set; }

    [JsonPropertyName("ts")]
    public string? TrackSeenUrl { get; set; }

    [JsonPropertyName("ci")]
    public bool IsConfirmed { get; init; }

    [JsonPropertyName("ns")]
    public string Subject { get; init; }

    [JsonPropertyName("nb")]
    public string? Body { get; init; }

    public static WebPushPayload Create(UserNotification notification, Guid configurationId)
    {
        var result = new WebPushPayload
        {
            IsConfirmed = notification.FirstConfirmed != null,
        };

        SimpleMapper.Map(notification, result);
        SimpleMapper.Map(notification.Formatting, result);

        // Compute the tracking links afterwards because the mapping would override it.
        result.ConfirmText = notification.Formatting.ConfirmText;
        result.ConfirmUrl = notification.ComputeConfirmUrl(Providers.WebPush, configurationId);
        result.TrackSeenUrl = notification.ComputeTrackSeenUrl(Providers.WebPush, configurationId);
        result.TrackDeliveredUrl = notification.ComputeTrackDeliveredUrl(Providers.WebPush, configurationId);

        return result;
    }
}
