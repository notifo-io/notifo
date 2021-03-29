// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.Webhook
{
    public sealed class WebhookJob
    {
        public UserNotification Notification { get; set; }

        public string Url { get; set; }

        public string ScheduleKey
        {
            get => $"{Notification.Id}_{Url}";
        }

        public WebhookJob()
        {
        }

        public WebhookJob(UserNotification notification, string url)
        {
            Notification = notification;

            Url = url;
        }
    }
}
