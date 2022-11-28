// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using System.Text;
using Notifo.Domain.Integrations;

namespace Notifo.Domain.UserNotifications;

public class BaseUserNotification : IIntegrationTarget
{
    public Guid Id { get; set; }

    public string EventId { get; set; }

    public string UserId { get; set; }

    public string UserLanguage { get; set; }

    public string AppId { get; set; }

    public string Topic { get; set; }

    public string? CorrelationId { get; set; }

    public string? Data { get; set; }

    public string? TrackDeliveredUrl { get; set; }

    public string? TrackSeenUrl { get; set; }

    public string? ConfirmUrl { get; set; }

    public bool Silent { get; set; }

    public bool Test { get; set; }

    public int? TimeToLiveInSeconds { get; set; }

    public ActivityContext EventActivity { get; set; }

    public ActivityContext UserEventActivity { get; set; }

    public ActivityContext UserNotificationActivity { get; set; }

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

    public string? ComputeTrackDeliveredUrl(string channel, Guid configurationId)
    {
        return ComputeUrl(TrackDeliveredUrl, channel, configurationId);
    }

    public string? ComputeTrackSeenUrl(string channel, Guid configurationId)
    {
        return ComputeUrl(TrackSeenUrl, channel, configurationId);
    }

    public string? ComputeConfirmUrl(string channel, Guid configurationId)
    {
        return ComputeUrl(ConfirmUrl, channel, configurationId);
    }

    private static string? ComputeUrl(string? url, string channel, Guid configurationId)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            var builder = new StringBuilder(url);

            var hasQuery = url.Contains('?', StringComparison.OrdinalIgnoreCase);

            void Append(string key, string value)
            {
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

            if (channel != null)
            {
                Append(nameof(channel), channel);
            }

            if (configurationId != default)
            {
                Append(nameof(configurationId), configurationId.ToString());
            }

            return builder.ToString();
        }

        return null;
    }
}
