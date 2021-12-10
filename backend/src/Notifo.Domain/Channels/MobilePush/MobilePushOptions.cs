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
        public string DeviceToken { get; set; }

        public MobileDeviceType DeviceType { get; set; }

        public bool IsConfirmed { get; set; }

        public bool Wakeup { get; set; }
    }
}
