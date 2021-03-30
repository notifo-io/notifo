// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Json;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Domain.Channels.WebPush
{
    public sealed class WebPushJob : IUserNotification
    {
        public Guid Id { get; set; }

        public string AppId { get; set; }

        public string EventId { get; set; }

        public string UserId { get; set; }

        public string Topic { get; set; }

        public string Payload { get; set; }

        public WebPushSubscription Subscription { get; set; }

        public bool IsImmediate { get; set; }

        public string ScheduleKey
        {
            get => $"{Id}_{Subscription.Endpoint}";
        }

        public WebPushJob()
        {
        }

        public WebPushJob(UserNotification notification, WebPushSubscription subscription, IJsonSerializer serializer)
        {
            Subscription = subscription;

            SimpleMapper.Map(notification, this);

            var payload = WebPushPayload.Create(notification, subscription.Endpoint);

            Payload = serializer.SerializeToString(payload);
        }
    }
}