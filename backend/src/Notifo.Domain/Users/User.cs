// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.MobilePush;
using Notifo.Domain.Channels.WebPush;
using Notifo.Domain.Counters;
using Notifo.Infrastructure.Collections;

namespace Notifo.Domain.Users
{
    public sealed record User
    {
        public string Id { get; private init; }

        public string AppId { get; private init; }

        public string UniqueId => "{AppId}_{Id}";

        public string ApiKey { get; init; }

        public string PreferredLanguage { get; init; } = "en";

        public string PreferredTimezone { get; init; } = "UTC";

        public string? FullName { get; init; }

        public string? EmailAddress { get; init; }

        public string? PhoneNumber { get; init; }

        public string? ThreemaId { get; init; }

        public string? TelegramUsername { get; init; }

        public string? TelegramChatId { get; init; }

        public bool RequiresWhitelistedTopics { get; init; }

        public ReadonlyList<string> AllowedTopics { get; init; } = ReadonlyList.Empty<string>();

        public ReadonlyList<MobilePushToken> MobilePushTokens { get; init; } = ReadonlyList.Empty<MobilePushToken>();

        public ReadonlyList<WebPushSubscription> WebPushSubscriptions { get; init; } = ReadonlyList.Empty<WebPushSubscription>();

        public NotificationSettings Settings { get; init; } = new NotificationSettings();

        public CounterMap Counters { get; init; } = new CounterMap();

        public static User Create(string appId, string userId)
        {
            return new User { Id = userId, AppId = appId };
        }
    }
}
