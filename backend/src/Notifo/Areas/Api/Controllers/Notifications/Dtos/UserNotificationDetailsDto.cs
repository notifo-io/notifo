// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Notifo.Domain.UserNotifications;

namespace Notifo.Areas.Api.Controllers.Notifications.Dtos
{
    public sealed class UserNotificationDetailsDto : UserNotificationDto
    {
        /// <summary>
        /// The channel details.
        /// </summary>
        [Required]
        public Dictionary<string, UserNotificationChannelDto> Channels { get; set; }

        /// <summary>
        /// The information when the notifcation was marked as confirmed.
        /// </summary>
        public HandledInfoDto? FirstConfirmed { get; set; }

        /// <summary>
        /// The information when the notifcation was marked as seen.
        /// </summary>
        public HandledInfoDto? FirstSeen { get; set; }

        public static UserNotificationDetailsDto FromDomainObjectAsDetails(UserNotification source)
        {
            var result = new UserNotificationDetailsDto();

            result.MapFrom(source);

            return result;
        }

        protected override void MapFrom(UserNotification source)
        {
            base.MapFrom(source);

            if (source.FirstConfirmed != null)
            {
                FirstConfirmed = HandledInfoDto.FromDomainObject(source.FirstConfirmed);
            }

            if (source.FirstSeen != null)
            {
                FirstSeen = HandledInfoDto.FromDomainObject(source.FirstSeen);
            }

            Channels = new Dictionary<string, UserNotificationChannelDto>();

            if (source.Channels != null)
            {
                foreach (var (key, value) in source.Channels)
                {
                    Channels[key] = UserNotificationChannelDto.FromDomainObject(value);
                }
            }
        }
    }
}
