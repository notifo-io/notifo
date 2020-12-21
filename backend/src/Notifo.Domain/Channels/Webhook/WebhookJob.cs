// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.Webhook
{
    public sealed class WebhookJob
    {
        public string Url { get; set; }

        public UserNotification Notification { get; set; }

        public string ScheduleKey
        {
            get { return Notification.Id.ToString(); }
        }

        public WebhookJob()
        {
        }

        public WebhookJob(UserNotification notification, App app)
        {
            Url = app.WebhookUrl!;

            Notification = notification;
        }
    }
}
