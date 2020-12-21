// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
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

        public HashSet<WebPushSubscription> Subscriptions { get; set; }

        public string ScheduleKey
        {
            get { return Id.ToString(); }
        }

        public WebPushJob()
        {
        }

        public WebPushJob(UserNotification notification, User user, IJsonSerializer serializer)
        {
            Subscriptions = user.WebPushSubscriptions;

            SimpleMapper.Map(notification, this);

            var payload = WebPushPayload.Create(notification);

            Payload = serializer.SerializeToString(payload);
        }
    }
}