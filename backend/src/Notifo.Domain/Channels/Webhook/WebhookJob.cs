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
        get => ComputeScheduleKey(Notification.Id);
    }

    public WebhookJob()
    {
    }

    public WebhookJob(UserNotification notification, ChannelContext context, string integrationId)
        : base(notification, context)
    {
        IntegrationId = integrationId;
    }

    public static string ComputeScheduleKey(Guid notificationId)
    {
        return notificationId.ToString();
    }
}
