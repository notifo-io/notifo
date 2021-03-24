// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Notifo.Domain.Channels.MobilePush;
using Notifo.Domain.Channels.WebPush;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;

namespace Notifo.Domain.Users
{
    public sealed class User
    {
        public string Id { get; private set; }

        public string AppId { get; set; }

        public string ApiKey { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string EmailAddress { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string PreferredLanguage { get; set; } = "en";

        public string PreferredTimezone { get; set; } = "UTC";

        public bool RequiresWhitelistedTopics { get; set; }

        public HashSet<string> AllowedTopics { get; set; } = new HashSet<string>();

        public HashSet<MobilePushToken> MobilePushTokens { get; set; } = new HashSet<MobilePushToken>();

        public HashSet<WebPushSubscription> WebPushSubscriptions { get; set; } = new HashSet<WebPushSubscription>();

        public NotificationSettings Settings { get; set; } = new NotificationSettings();

        public CounterMap Counters { get; set; } = new CounterMap();

        public static User Create(string appId, string userId)
        {
            var user = new User { AppId = appId, Id = userId };

            user.ApiKey = RandomHash.New();

            return user;
        }

        public string ToFullId()
        {
            return $"{AppId}_{Id}";
        }
    }
}
