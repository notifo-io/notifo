// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Channels.Webhook.Integrations;
using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.Webhook
{
    public sealed class WebhookJob : IChannelJob
    {
        public UserNotification Notification { get; init; }

        public WebhookDefinition Webhook { get; init; }

        public string WebhookId { get; init; }

        public bool IsConfirmed { get; init; }

        public bool IsUpdate { get; init; }

        public ChannelCondition Condition { get; init; }

        public Duration Delay { get; init; }

        Guid IChannelJob.NotificationId
        {
            get => Notification.Id;
        }

        public string Configuration
        {
            get => WebhookId;
        }

        public string ScheduleKey
        {
            get => Notification.Id.ToString();
        }

        public WebhookJob()
        {
        }

        public WebhookJob(UserNotification notification, ChannelSetting setting, string webhookId, WebhookDefinition webhook, bool isUpdate)
        {
            Condition = setting.Condition;
            Delay = Duration.FromSeconds(setting?.DelayInSeconds ?? 0);
            IsConfirmed = notification.FirstConfirmed != null;
            IsUpdate = isUpdate;
            Notification = notification;
            Webhook = webhook;
            WebhookId = webhookId;
        }
    }
}
