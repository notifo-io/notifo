// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;
using Notifo.Domain.Utils;

namespace Notifo.Domain.Channels
{
    public static class ChannelExtensions
    {
        public static string HtmlTrackingLink(this BaseUserNotification notification, string emailAddress)
        {
            var trackingUrl = notification.ComputeTrackSeenUrl(Providers.Email, emailAddress);
            var trackingLink = $"<img height=\"0\" width=\"0\" style=\"width: 0px; height: 0px; position: absolute; visibility: hidden;\" src=\"{trackingUrl}\" />";

            return trackingLink;
        }

        public static string? ConfirmText(this BaseUserNotification notification)
        {
            return notification.Formatting.ConfirmText;
        }

        public static string? ConfirmUrl(this BaseUserNotification notification)
        {
            return notification.ConfirmUrl;
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
    }
}
