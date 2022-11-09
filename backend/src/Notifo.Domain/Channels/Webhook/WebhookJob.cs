// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.Webhook.Integrations;
using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.Webhook;

public sealed class WebhookJob : ChannelJob
{
    public UserNotification Notification { get; init; }

    public WebhookDefinition Webhook { get; init; }

    public bool IsConfirmed { get; init; }

    public string ScheduleKey
    {
        get => Notification.Id.ToString();
    }

    public WebhookJob()
    {
    }

    public WebhookJob(UserNotification notification, ChannelSetting setting, Guid configurationId, WebhookDefinition webhook, bool isUpdate)
        : base(notification, setting, configurationId, isUpdate, Providers.Webhook)
    {
        Notification = notification;
        IsConfirmed = notification.FirstConfirmed != null;
        IsUpdate = isUpdate;
        Webhook = webhook;
    }
}
