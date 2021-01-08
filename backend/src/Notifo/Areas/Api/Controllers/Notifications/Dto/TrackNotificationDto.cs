// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Notifo.Areas.Api.Controllers.Notifications.Dto
{
    public sealed class TrackNotificationDto
    {
        public Guid? Confirmed { get; set; }

        public Guid[]? Seen { get; set; }

        public string? Channel { get; set; }
    }
}
