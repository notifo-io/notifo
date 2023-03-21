// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Json;

namespace Notifo.Domain.Channels.WebPush;

public sealed class WebPushJob : ChannelJob
{
    public WebPushSubscription Subscription { get; init; }

    public string Payload { get; init; }

    public string ScheduleKey
    {
        get => Notification.Id.ToString();
    }

    public WebPushJob()
    {
    }

    public WebPushJob(UserNotification notification, ChannelContext context, WebPushSubscription subscription, IJsonSerializer serializer)
        : base(notification, context)
    {
        var payload = WebPushPayload.Create(notification, context.ConfigurationId);

        // Serialize the payload directly, so we do not have it multiple times in case of a retry.
        Payload = serializer.SerializeToString(payload);

        Subscription = subscription;
    }
}
