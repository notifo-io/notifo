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

namespace Notifo.Domain.Users;

public sealed record User(string AppId, string Id, Instant Created)
{
    public string UniqueId => BuildId(AppId, Id);

    public string ApiKey { get; init; }

    public string PreferredLanguage { get; init; } = "en";

    public string PreferredTimezone { get; init; } = "UTC";

    public string? FullName { get; init; }

    public string? EmailAddress { get; init; }

    public string? PhoneNumber { get; init; }

    public bool RequiresWhitelistedTopics { get; init; }

    public Instant LastUpdate { get; init; }

    public ReadonlyList<string> AllowedTopics { get; init; } = [];

    public ReadonlyDictionary<string, string> Properties { get; init; } = ReadonlyDictionary.Empty<string, string>();

    public ReadonlyDictionary<string, string>? SystemProperties { get; init; }

    public ReadonlyList<MobilePushToken> MobilePushTokens { get; init; } = [];

    public ReadonlyList<WebPushSubscription> WebPushSubscriptions { get; init; } = [];

    public Scheduling? Scheduling { get; init; }

    public ChannelSettings Settings { get; init; } = [];

    public CounterMap Counters { get; init; } = [];

    public static string BuildId(string appId, string userId)
    {
        return $"{appId}_{userId}";
    }
}
