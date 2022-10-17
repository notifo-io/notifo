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
        private readonly BaseUserNotification notification;
        private readonly string? emailAddress;
        private readonly IImageFormatter imageFormatter;
        private string? confirmUrl;
        private string? imageLarge;
        private string? imageSmall;
        private string? trackDeliveredUrl;
        private string? trackSeenUrl;

        public string Subject => notification.Formatting.Subject;

        public string? Body => OrNull(notification.Formatting.Body);

        public string? LinkUrl => OrNull(notification.Formatting.LinkUrl);

        public string? LinkText => OrNull(notification.Formatting.LinkText);

        public string? ConfirmText => OrNull(notification.Formatting.ConfirmText);

        public string? TrackSeenUrl
        {
            get => trackSeenUrl ??= notification.ComputeTrackSeenUrl(Providers.Email, emailAddress);
        }

        public string? TrackDeliveredUrl
        {
            get => trackDeliveredUrl ??= notification.ComputeTrackDeliveredUrl(Providers.Email, emailAddress);
        }

        public string? ConfirmUrl
        {
            get => confirmUrl ??= notification.ComputeConfirmUrl(Providers.Email, emailAddress);
        }

        public string? ImageSmall
        {
            get
            {
                if (string.IsNullOrWhiteSpace(notification.Formatting.ImageSmall))
                {
                    return null;
                }

                return imageSmall ??= imageFormatter.Format(notification.Formatting.ImageSmall, "EmailSmall", true);
            }
        }

        public string? ImageLarge
        {
            get
            {
                if (string.IsNullOrWhiteSpace(notification.Formatting.ImageLarge))
                {
                    return null;
                }

                return imageLarge ??= imageFormatter.Format(notification.Formatting.ImageLarge, "EmailLarge", true);
            }
        }

        public EmailNotification(BaseUserNotification notification, string? emailAddress, IImageFormatter imageFormatter)
        {
            this.notification = notification;
            this.emailAddress = emailAddress;
            this.imageFormatter = imageFormatter;
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
