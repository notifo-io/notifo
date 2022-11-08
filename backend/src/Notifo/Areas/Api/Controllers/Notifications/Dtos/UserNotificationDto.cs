// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Domain;
using Notifo.Domain.Channels;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Notifications.Dtos
{
    public sealed class UserNotificationDto : UserNotificationBaseDto
    {
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

        public static UserNotificationDto FromDomainObject(UserNotification source)
        {
            var result = new UserNotificationDto
            {
                IsConfirmed = source.FirstConfirmed != null,
                IsSeen = source.FirstSeen != null
            };

            SimpleMapper.Map(source, result);
            SimpleMapper.Map(source.Formatting, result);

            result.TrackingToken = new TrackingToken(source.Id, Providers.Web).ToParsableString();

            return result;
        }
    }
}
