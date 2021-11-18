// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Notifications.Dtos
{
    public sealed class UserNotificationDetailsDto : UserNotificationBaseDto
    {
        /// <summary>
        /// The channel details.
        /// </summary>
        [Required]
        public Dictionary<string, UserNotificationChannelDto> Channels { get; set; }

        /// <summary>
        /// The information when the notifcation was marked as deliverd.
        /// </summary>
        public HandledInfoDto? FirstDelivered { get; set; }

        /// <summary>
        /// The information when the notifcation was marked as seen.
        /// </summary>
        public HandledInfoDto? FirstSeen { get; set; }

        /// <summary>
        /// The information when the notifcation was marked as confirmed.
        /// </summary>
        public HandledInfoDto? FirstConfirmed { get; set; }

        public static UserNotificationDetailsDto FromDomainObjectAsDetails(UserNotification source)
        {
            var result = new UserNotificationDetailsDto();

            SimpleMapper.Map(source, result);
            SimpleMapper.Map(source.Formatting, result);

            if (source.FirstDelivered != null)
            {
                result.FirstDelivered = HandledInfoDto.FromDomainObject(source.FirstDelivered);
            }

            if (source.FirstSeen != null)
            {
                result.FirstSeen = HandledInfoDto.FromDomainObject(source.FirstSeen);
            }

            if (source.FirstConfirmed != null)
            {
                result.FirstConfirmed = HandledInfoDto.FromDomainObject(source.FirstConfirmed);
            }

            result.Channels = new Dictionary<string, UserNotificationChannelDto>();

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
