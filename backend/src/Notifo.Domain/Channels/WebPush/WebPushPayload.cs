// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Text.Json.Serialization;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Domain.Channels.WebPush
{
    public sealed class WebPushPayload
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("cu")]
        public string? ConfirmUrl { get; set; }

        [JsonPropertyName("ct")]
        public string? ConfirmText { get; set; }

        [JsonPropertyName("is")]
        public string? ImageSmall { get; set; }

        [JsonPropertyName("il")]
        public string? ImageLarge { get; set; }

        [JsonPropertyName("lt")]
        public string? LinkText { get; set; }

        [JsonPropertyName("lu")]
        public string? LinkUrl { get; set; }

        [JsonPropertyName("tu")]
        public string? TrackingUrl { get; set; }

        [JsonPropertyName("ci")]
        public bool IsConfirmed { get; set; }

        [JsonPropertyName("ns")]
        public string Subject { get; set; }

        [JsonPropertyName("nb")]
        public string? Body { get; set; }

        public static WebPushPayload Create(UserNotification notification)
        {
            var result = new WebPushPayload();

            SimpleMapper.Map(notification, result);
            SimpleMapper.Map(notification.Formatting, result);

            result.IsConfirmed = notification.IsConfirmed != null;

            return result;
        }
    }
}
