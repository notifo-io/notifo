// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Notifications.Dtos
{
    public class UserNotificationDto
    {
        /// <summary>
        /// The id of the notification.
        /// </summary>
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        /// The subject of the notification in the language of the user.
        /// </summary>
        [Required]
        public string Subject { get; set; }

        /// <summary>
        /// True when the notification is silent.
        /// </summary>
        [Required]
        public bool Silent { get; set; }

        /// <summary>
        /// True when the notification has been delivered at least once.
        /// </summary>
        [Required]
        public bool IsDelivered { get; set; }

        /// <summary>
        /// True when the notification has been seen at least once.
        /// </summary>
        [Required]
        public bool IsSeen { get; set; }

        /// <summary>
        /// True when the notification has been confirmed at least once.
        /// </summary>
        [Required]
        public bool IsConfirmed { get; set; }

        /// <summary>
        /// The timestamp when the notification has been created.
        /// </summary>
        [Required]
        public Instant Created { get; set; }

        /// <summary>
        /// The timestamp when the notification has been updated.
        /// </summary>
        [Required]
        public Instant Updated { get; set; }

        /// <summary>
        /// The optional body text.
        /// </summary>
        public string? Body { get; set; }

        /// <summary>
        /// The optional link to the small image.
        /// </summary>
        public string? ImageSmall { get; set; }

        /// <summary>
        /// The optional link to the large image.
        /// </summary>
        public string? ImageLarge { get; set; }

        /// <summary>
        /// The tracking url that needs to be invoked to mark the notification as seen.
        /// </summary>
        public string? TrackSeenUrl { get; set; }

        /// <summary>
        /// The tracking url that needs to be invoked to mark the notification as delivered.
        /// </summary>
        public string? TrackDeliveredUrl { get; set; }

        /// <summary>
        /// An optional link.
        /// </summary>
        public string? LinkUrl { get; set; }

        /// <summary>
        /// The link text.
        /// </summary>
        public string? LinkText { get; set; }

        /// <summary>
        /// The text for the confirm button.
        /// </summary>
        public string? ConfirmText { get; set; }

        /// <summary>
        /// The tracking url that needs to be invoked to mark the notification as confirmed.
        /// </summary>
        public string? ConfirmUrl { get; set; }

        /// <summary>
        /// Optional data, usually a json object.
        /// </summary>
        public string? Data { get; set; }

        public static UserNotificationDto FromDomainObject(UserNotification source)
        {
            var result = new UserNotificationDto();

            result.MapFrom(source);

            return result;
        }

        protected virtual void MapFrom(UserNotification source)
        {
            IsDelivered = source.FirstDelivered != null;
            IsSeen = source.FirstSeen != null;
            IsConfirmed = source.FirstConfirmed != null;

            SimpleMapper.Map(source, this);
            SimpleMapper.Map(source.Formatting, this);
        }
    }
}
