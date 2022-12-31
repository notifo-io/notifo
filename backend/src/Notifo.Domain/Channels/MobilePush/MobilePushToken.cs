// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Integrations;

namespace Notifo.Domain.Channels.MobilePush;

public sealed record MobilePushToken
{
    public string Token { get; init; }

    public string? DeviceIdentifier { get; init; }

    public Instant LastWakeup { get; init; }

    public MobileDeviceType DeviceType { get; init; }
}
