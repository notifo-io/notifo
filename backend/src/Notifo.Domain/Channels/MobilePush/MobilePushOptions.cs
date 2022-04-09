// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels.MobilePush
{
    public struct MobilePushOptions
    {
        public string DeviceToken { get; init; }

        public MobileDeviceType DeviceType { get; init; }

        public bool IsConfirmed { get; init; }

        public bool Wakeup { get; init; }
    }
}
