// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.Email.Formatting
{
    public static class Helpers
    {
        public static string BuildTrackingLink(BaseUserNotification notification, string emailAddress)
        {
            var trackingUrl = notification.ComputeTrackSeenUrl(Providers.Email, emailAddress);
            var trackingLink = $"<img height=\"0\" width=\"0\" style=\"width: 0px; height: 0px; position: absolute; visibility: hidden;\" src=\"{trackingUrl}\" />";

            return trackingLink;
        }
    }
}
