// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Domain.Channels.WebPush
{
    public sealed class WebPushPayload
    {
        [JsonPropertyName("id")]
        public string Id { get; init; }

        [JsonPropertyName("cu")]
        public string? ConfirmUrl { get; init; }

        [JsonPropertyName("ct")]
        public string? ConfirmText { get; init; }

        [JsonPropertyName("is")]
        public string? ImageSmall { get; init; }

        [JsonPropertyName("il")]
        public string? ImageLarge { get; init; }

        [JsonPropertyName("lt")]
        public string? LinkText { get; init; }

        [JsonPropertyName("lu")]
        public string? LinkUrl { get; init; }

        [JsonPropertyName("td")]
        public string? TrackDeliveredUrl { get; init; }

        [JsonPropertyName("ts")]
        public string? TrackSeenUrl { get; init; }

        [JsonPropertyName("ci")]
        public bool IsConfirmed { get; init; }

        [JsonPropertyName("ns")]
        public string Subject { get; init; }

        [JsonPropertyName("nb")]
        public string? Body { get; init; }

        public static WebPushPayload Create(UserNotification notification, string endpoint)
        {
            var result = new WebPushPayload
            {
                ConfirmText = notification.Formatting.ConfirmText,
                ConfirmUrl = notification.ComputeConfirmUrl(Providers.WebPush, endpoint),
                IsConfirmed = notification.FirstConfirmed != null,
                TrackDeliveredUrl = notification.ComputeTrackDeliveredUrl(Providers.WebPush, endpoint),
                TrackSeenUrl = notification.ComputeTrackSeenUrl(Providers.WebPush, endpoint)
            };

            SimpleMapper.Map(notification, result);
            SimpleMapper.Map(notification.Formatting, result);

            return result;
        }
    }
}
