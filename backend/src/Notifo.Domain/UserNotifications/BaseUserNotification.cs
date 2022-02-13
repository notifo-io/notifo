// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using System.Text;
using Notifo.Domain.Integrations;

namespace Notifo.Domain.UserNotifications
{
    public class BaseUserNotification : IUserNotification, IIntegrationTarget
    {
        public Guid Id { get; set; }

        public string EventId { get; set; }

        public string UserId { get; set; }

        public string UserLanguage { get; set; }

        public string AppId { get; set; }

        public string Topic { get; set; }

        public string? Data { get; set; }

        public string? TrackDeliveredUrl { get; set; }

        public string? TrackSeenUrl { get; set; }

        public string? ConfirmUrl { get; set; }

        public bool Silent { get; set; }

        public bool Test { get; set; }

        public int? TimeToLiveInSeconds { get; set; }

        public ActivityContext UserEventActivity { get; set; }

        public ActivityContext EventActivity { get; set; }

        public ActivityContext NotificationActivity { get; set; }

        public NotificationProperties? Properties { get; set; }

        public NotificationFormatting<string> Formatting { get; set; }

        IEnumerable<KeyValuePair<string, object>> IIntegrationTarget.Properties
        {
            get
            {
                if (Properties != null)
                {
                    yield return new KeyValuePair<string, object>("properties", Properties);
                }
            }
        }

        public string? ComputeTrackDeliveredUrl(string channel, string? deviceIdentifier)
        {
            return ComputeUrl(TrackDeliveredUrl, channel, deviceIdentifier);
        }

        public string? ComputeTrackSeenUrl(string channel, string? deviceIdentifier)
        {
            return ComputeUrl(TrackSeenUrl, channel, deviceIdentifier);
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

                var hasQuery = url.Contains('?', StringComparison.OrdinalIgnoreCase);

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
                    builder.Append(Uri.EscapeDataString(value));

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
