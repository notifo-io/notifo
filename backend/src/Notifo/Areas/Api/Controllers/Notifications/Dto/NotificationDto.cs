// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Notifications.Dto
{
    public sealed class NotificationDto
    {
        public Guid Id { get; set; }

        public string Subject { get; set; }

        public string? Body { get; set; }

        public string? ImageSmall { get; set; }

        public string? ImageLarge { get; set; }

        public string? TrackingUrl { get; set; }

        public string? LinkUrl { get; set; }

        public string? LinkText { get; set; }

        public string? ConfirmText { get; set; }

        public string? ConfirmUrl { get; set; }

        public string? Data { get; set; }

        public bool Silent { get; set; }

        public bool IsConfirmed { get; set; }

        public bool IsSeen { get; set; }

        public Instant Created { get; set; }

        public static NotificationDto FromNotification(UserNotification notification)
        {
            var result = new NotificationDto
            {
                IsConfirmed = notification.IsConfirmed != null,
                IsSeen = notification.IsSeen != null
            };

            SimpleMapper.Map(notification, result);
            SimpleMapper.Map(notification.Formatting, result);

            return result;
        }
    }
}
