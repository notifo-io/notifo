// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using NodaTime;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Json;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Domain.Channels.WebPush
{
    public sealed class WebPushJob : IUserNotification, IChannelJob
    {
        public Guid Id { get; init; }

        public string AppId { get; init; }

        public string EventId { get; init; }

        public string UserId { get; init; }

        public string Topic { get; init; }

        public string Payload { get; init; }

        public WebPushSubscription Subscription { get; init; }

        public ActivityContext UserEventActivity { get; init; }

        public ActivityContext EventActivity { get; init; }

        public ActivityContext NotificationActivity { get; init; }

        public bool IsConfirmed { get; init; }

        public bool IsUpdate { get; init; }

        public ChannelCondition Condition { get; init; }

        public Duration Delay { get; init; }

        Guid IChannelJob.NotificationId
        {
            get => Id;
        }

        public string Configuration
        {
            get => Subscription.Endpoint;
        }

        public string ScheduleKey
        {
            get => $"{Id}_{Subscription.Endpoint}";
        }

        public WebPushJob()
        {
        }

        public WebPushJob(UserNotification notification, ChannelSetting setting, WebPushSubscription subscription, IJsonSerializer serializer, bool isUpdate)
        {
            SimpleMapper.Map(notification, this);

            var payload = WebPushPayload.Create(notification, subscription.Endpoint);

            Condition = setting.Condition;
            Delay = Duration.FromSeconds(setting?.DelayInSeconds ?? 0);
            IsConfirmed = notification.FirstConfirmed != null;
            IsUpdate = isUpdate;
            Payload = serializer.SerializeToString(payload);
            Subscription = subscription;
        }
    }
}
