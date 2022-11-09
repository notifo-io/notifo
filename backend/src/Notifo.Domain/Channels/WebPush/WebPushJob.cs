// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Json;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Domain.Channels.WebPush;

public sealed class WebPushJob : ChannelJob
{
    public WebPushSubscription Subscription { get; init; }

    public string Payload { get; init; }

    public ActivityContext EventActivity { get; init; }

    public ActivityContext UserEventActivity { get; init; }

    public ActivityContext UserNotificationActivity { get; init; }

    public bool IsConfirmed { get; init; }

    public string ScheduleKey
    {
        get => ComputeScheduleKey(Tracking.UserNotificationId, Subscription.Endpoint);
    }

    public WebPushJob()
    {
    }

    public WebPushJob(UserNotification notification, ChannelSetting setting, Guid configurationId, WebPushSubscription subscription, IJsonSerializer serializer, bool isUpdate)
        : base(notification, setting, configurationId, isUpdate, Providers.WebPush)
    {
        SimpleMapper.Map(notification, this);

        var payload = WebPushPayload.Create(notification, configurationId);

        IsConfirmed = notification.FirstConfirmed != null;
        IsUpdate = isUpdate;
        Payload = serializer.SerializeToString(payload);
        Subscription = subscription;
    }

    public static string ComputeScheduleKey(Guid notificationId, string endpoint)
    {
        return $"{notificationId}_{endpoint}";
    }
}
