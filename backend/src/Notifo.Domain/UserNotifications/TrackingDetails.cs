// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.UserNotifications
{
    public struct TrackingDetails
    {
        public string? Channel { get; set; }

        public string? DeviceIdentifier { get; set; }

        public TrackingDetails(string? channel, string? deviceIdentifier)
        {
            Channel = channel;

            DeviceIdentifier = deviceIdentifier;
        }
    }
}
