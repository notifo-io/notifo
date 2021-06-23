// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Reflection;

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
        public HandledInfoDto? Confirmed { get; set; }

        /// <summary>
        /// The information when the notifcation was marked as seen.
        /// </summary>
        public HandledInfoDto? Seen { get; set; }

        public static UserNotificationDetailsDto FromDomainObjectAsDetails(UserNotification source)
        {
            var result = new UserNotificationDetailsDto
            {
                Channels = new Dictionary<string, UserNotificationChannelDto>(),
                IsConfirmed = source.IsConfirmed != null,
                IsSeen = source.IsSeen != null
            };

            SimpleMapper.Map(source, result);
            SimpleMapper.Map(source.Formatting, result);

            if (source.IsConfirmed != null)
            {
                result.Confirmed = HandledInfoDto.FromDomainObject(source.IsConfirmed);
            }

            if (source.IsSeen != null)
            {
                result.Seen = HandledInfoDto.FromDomainObject(source.IsSeen);
            }

            if (source.Channels != null)
            {
                foreach (var (key, value) in source.Channels)
                {
                    result.Channels[key] = UserNotificationChannelDto.FromDomainObject(value);
                }
            }

            return result;
        }
    }
}
