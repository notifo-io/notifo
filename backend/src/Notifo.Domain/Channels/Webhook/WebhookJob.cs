// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.Webhook.Integrations;
using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.Webhook
{
    public sealed class WebhookJob
    {
        public UserNotification Notification { get; init; }

        public WebhookDefinition Webhook { get; init; }

        public string WebhookId { get; init; }

        public bool IsUpdate { get; init; }

        public string ScheduleKey
        {
            get => Notification.Id.ToString();
        }

        public WebhookJob()
        {
        }

        public WebhookJob(UserNotification notification, string webhookId, WebhookDefinition webhook)
        {
            Notification = notification;

            WebhookId = webhookId;
            Webhook = webhook;
        }
    }
}
