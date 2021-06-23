// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Notifo.Domain.UserNotifications;

namespace Notifo.Areas.Api.Controllers.Notifications.Dtos
{
    public sealed class TrackNotificationDto
    {
        /// <summary>
        /// The id of the noitifications to mark as confirmed.
        /// </summary>
        public Guid? Confirmed { get; set; }

        /// <summary>
        /// The id of the noitifications to mark as seen.
        /// </summary>
        public Guid[]? Seen { get; set; }

        /// <summary>
        /// The channel name.
        /// </summary>
        public string? Channel { get; set; }

        /// <summary>
        /// The device identifier.
        /// </summary>
        public string? DeviceIdentifier { get; set; }

        public TrackingDetails ToDetails()
        {
            return new TrackingDetails { Channel = Channel, DeviceIdentifier = DeviceIdentifier };
        }
    }
}
