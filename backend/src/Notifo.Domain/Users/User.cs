// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Channels.MobilePush;
using Notifo.Domain.Channels.WebPush;
using Notifo.Domain.Counters;
using Notifo.Infrastructure.Collections;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Users
{
    public sealed record User(string AppId, string Id, Instant Created)
    {
        public string UniqueId => $"{AppId}_{Id}";

        public string ApiKey { get; init; }

        public string PreferredLanguage { get; init; } = "en";

        public string PreferredTimezone { get; init; } = "UTC";

        public string? FullName { get; init; }

        public string? EmailAddress { get; init; }

        public string? PhoneNumber { get; init; }

        public bool RequiresWhitelistedTopics { get; init; }

        public Instant LastUpdate { get; init; }

        public ReadonlyList<string> AllowedTopics { get; init; } = ReadonlyList.Empty<string>();

        public ReadonlyDictionary<string, string> Properties { get; init; } = ReadonlyDictionary.Empty<string, string>();

        public ReadonlyList<MobilePushToken> MobilePushTokens { get; init; } = ReadonlyList.Empty<MobilePushToken>();

        public ReadonlyList<WebPushSubscription> WebPushSubscriptions { get; init; } = ReadonlyList.Empty<WebPushSubscription>();

        public NotificationSettings Settings { get; init; } = new NotificationSettings();

        public CounterMap Counters { get; init; } = new CounterMap();
    }
}
