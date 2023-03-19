// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure;

namespace Notifo.Domain.UserNotifications;

public class BaseUserNotification : SimpleNotification, IIntegrationTarget
{
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

    public List<SimpleNotification>? ChildNotifications { get; set; }

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
        return url.AppendQueries(nameof(channel), channel, nameof(configurationId), configurationId);
    }
}
