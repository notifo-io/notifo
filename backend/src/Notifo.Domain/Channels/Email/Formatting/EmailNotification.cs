// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;
using Notifo.Domain.Utils;

namespace Notifo.Domain.Channels.Email.Formatting
{
    public sealed class EmailNotification
    {
        public string Subject { get; set; }

        public string? Body { get; set; }

        public string? ConfirmText { get; set; }

        public string? ImageSmall { get; set; }

        public string? ImageLarge { get; set; }

        public string? LinkUrl { get; set; }

        public string? LinkText { get; set; }

        public string? TrackingUrl { get; set; }

        public string? ConfirmUrl { get; set; }

        public static EmailNotification Create(UserNotification notification, IImageFormatter imageFormatter)
        {
            var result = new EmailNotification
            {
                Body = OrNull(notification.Formatting.Body),
                LinkText = OrNull(notification.Formatting.LinkText),
                LinkUrl = OrNull(notification.Formatting.LinkUrl),
                TrackingUrl = notification.ComputeTrackingUrl(Providers.Email),
                Subject = notification.Formatting.Subject,
            };

            if (!string.IsNullOrWhiteSpace(notification.Formatting.ImageLarge))
            {
                result.ImageLarge = imageFormatter.Format(notification.Formatting.ImageLarge, "EmailLarge");
            }

            if (!string.IsNullOrWhiteSpace(notification.Formatting.ImageSmall))
            {
                result.ImageSmall = imageFormatter.Format(notification.Formatting.ImageSmall, "EmailLarge");
            }

            if (notification.Formatting.ConfirmMode == ConfirmMode.Explicit)
            {
                result.ConfirmText = OrNull(notification.Formatting.ConfirmText);
                result.ConfirmUrl = notification.ComputeConfirmUrl(Providers.Email);
            }

            return result;
        }

        private static string? OrNull(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value;
        }
    }
}
