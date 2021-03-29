// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using NodaTime;

namespace Notifo.Domain.UserNotifications
{
    public sealed class UserNotification : IUserNotification
    {
        public Guid Id { get; set; }

        public string EventId { get; set; }

        public string UserId { get; set; }

        public string UserLanguage { get; set; }

        public string AppId { get; set; }

        public string Topic { get; set; }

        public string? Data { get; set; }

        public string? TrackingUrl { get; set; }

        public string? ConfirmUrl { get; set; }

        public bool Silent { get; set; }

        public bool IsDeleted { get; set; }

        public HandledInfo? IsConfirmed { get; set; }

        public HandledInfo? IsSeen { get; set; }

        public Instant Created { get; set; }

        public Instant Updated { get; set; }

        public NotificationFormatting<string> Formatting { get; set; }

        public Dictionary<string, UserNotificationChannel> Channels { get; set; }

        public string? ComputeTrackingUrl(string channel)
        {
            return ComputeUrl(TrackingUrl, channel);
        }

        public string? ComputeConfirmUrl(string channel)
        {
            return ComputeUrl(ConfirmUrl, channel);
        }

        private static string? ComputeUrl(string? url, string channel)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                if (url.Contains("?", StringComparison.OrdinalIgnoreCase))
                {
                    return $"{url}&channel={channel}";
                }

                return $"{url}?channel={channel}";
            }

            return null;
        }
    }
}
