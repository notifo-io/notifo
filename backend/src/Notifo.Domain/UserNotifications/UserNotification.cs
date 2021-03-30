// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text;
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

        public string? ComputeTrackingUrl(string channel, string? deviceIdentifier)
        {
            return ComputeUrl(TrackingUrl, channel, deviceIdentifier);
        }

        public string? ComputeConfirmUrl(string channel, string? deviceIdentifier)
        {
            return ComputeUrl(ConfirmUrl, channel, deviceIdentifier);
        }

        private static string? ComputeUrl(string? url, string channel, string? deviceIdentifier)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                var builder = new StringBuilder(url);

                var hasQuery = url.Contains("?", StringComparison.OrdinalIgnoreCase);

                void Append(string key, string? value)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return;
                    }

                    if (hasQuery)
                    {
                        builder.Append('&');
                    }
                    else
                    {
                        builder.Append('?');
                    }

                    builder.Append(key);
                    builder.Append('=');
                    builder.Append(Uri.EscapeUriString(value));

                    hasQuery = true;
                }

                Append(nameof(channel), channel);
                Append(nameof(deviceIdentifier), deviceIdentifier);

                return builder.ToString();
            }

            return null;
        }
    }
}
