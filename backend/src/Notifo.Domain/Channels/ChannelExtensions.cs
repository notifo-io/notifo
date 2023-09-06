// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Notifo.Domain.Integrations;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Utils;

namespace Notifo.Domain.Channels;

public static class ChannelExtensions
{
    public static string HtmlTrackingLink(this BaseUserNotification notification, Guid configurationId)
    {
        var trackingUrl = notification.ComputeTrackSeenUrl(Providers.Email, configurationId);
        var trackingLink = $"<img height=\"0\" width=\"0\" style=\"width: 0px; height: 0px; position: absolute; visibility: hidden;\" src=\"{trackingUrl}\" />";

        return trackingLink;
    }

    public static string? ImageSmall(this BaseUserNotification notification, IImageFormatter imageFormatter, string preset)
    {
        return imageFormatter.AddPreset(notification.Formatting.ImageSmall, preset);
    }

    public static string? ImageLarge(this BaseUserNotification notification, IImageFormatter imageFormatter, string preset)
    {
        return imageFormatter.AddPreset(notification.Formatting.ImageSmall, preset);
    }

    public static string Subject(this BaseUserNotification notification, bool asHtml = false)
    {
        var formatting = notification.Formatting;

        var subject = formatting.Subject!;

        if (asHtml && !string.IsNullOrWhiteSpace(formatting.LinkUrl))
        {
            subject = $"<a href=\"{formatting.LinkUrl}\" target=\"_blank\" rel=\"noopener\">{subject}</a>";
        }

        return subject;
    }

    public static string? BodyWithLink(this BaseUserNotification notification, bool asHtml = false)
    {
        var formatting = notification.Formatting;

        var body = formatting.Body;

        if (asHtml && !string.IsNullOrWhiteSpace(formatting.LinkText) && !string.IsNullOrWhiteSpace(formatting.LinkUrl))
        {
            if (body?.Length > 0)
            {
                return $"{body} <a href=\"{formatting.LinkUrl}\">{formatting.LinkText}</a>";
            }
            else
            {
                return $"<a href=\"{formatting.LinkUrl}\">{formatting.LinkText}</a>";
            }
        }

        if (!string.IsNullOrWhiteSpace(formatting.LinkUrl))
        {
            if (body?.Length > 0)
            {
                return $"{body} {formatting.LinkUrl}";
            }
            else
            {
                return formatting.LinkUrl;
            }
        }

        return body;
    }

    public static T Enrich<T>(this T message, ChannelJob job, string channelName) where T : BaseMessage
    {
        var notification = job.Notification;

        message.IsConfirmed = job.IsConfirmed;
        message.IsUpdate = job.IsUpdate;
        message.NotificationId = job.Notification.Id;
        message.Silent = notification.Silent;
        message.TrackDeliveredUrl = notification.ComputeTrackDeliveredUrl(channelName, job.ConfigurationId);
        message.TrackSeenUrl = notification.ComputeTrackSeenUrl(channelName, job.ConfigurationId);
        message.TrackingToken = new TrackingToken(job.Notification.Id, channelName, job.ConfigurationId).ToParsableString();
        message.UserId = job.Notification.UserId;
        message.UserLanguage = job.Notification.UserLanguage;

        return message;
    }

    public static IEnumerable<ActivityLink> Links(this ChannelJob job)
    {
        if (job.Notification.UserEventActivity != default)
        {
            yield return new ActivityLink(job.Notification.UserEventActivity);
        }

        if (job.Notification.EventActivity != default)
        {
            yield return new ActivityLink(job.Notification.EventActivity);
        }

        if (job.Notification.UserNotificationActivity != default)
        {
            yield return new ActivityLink(job.Notification.UserNotificationActivity);
        }
    }
}
