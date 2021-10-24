// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Domain.Channels.MobilePush
{
    public sealed record MobilePushToken
    {
        public string Token { get; init; }

        public MobileDeviceType DeviceType { get; init; }

        public Instant LastWakeup { get; init; }
    }
}
