// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Domain.Channels.MobilePush
{
    public sealed class MobilePushToken
    {
        public string Token { get; set; }

        public MobileDeviceType DeviceType { get; set; }

        public Instant LastWakeup { get; set; }
    }
}
