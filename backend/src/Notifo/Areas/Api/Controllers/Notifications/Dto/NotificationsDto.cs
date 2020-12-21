// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Notifo.Areas.Api.Controllers.Notifications.Dto
{
    public sealed class NotificationsDto
    {
        [Required]
        public long Etag { get; set; }

        [Required]
        public NotificationDto[] Notifications { get; set; }
    }
}
