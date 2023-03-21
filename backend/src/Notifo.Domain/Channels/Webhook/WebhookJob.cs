// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.Webhook;

public sealed class WebhookJob : ChannelJob
{
    public string IntegrationId { get; set; }

    public string ScheduleKey
    {
        get => Notification.Id.ToString();
    }

    public WebhookJob()
    {
    }

    public WebhookJob(UserNotification notification, ChannelContext context, string integrationId)
        : base(notification, context)
    {
        IntegrationId = integrationId;
    }
}
