// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using NodaTime;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Notifications.Dtos
{
    public sealed class UserNotificationChannelDto
    {
        /// <summary>
        /// The notification settings.
        /// </summary>
        [Required]
        public ChannelSettingDto Setting { get; set; }

        /// <summary>
        /// The status per token or configuration.
        /// </summary>
        [Required]
        public Dictionary<string, ChannelSendInfoDto> Status { get; set; }

        /// <summary>
        /// The first time the notification has been marked as delivered for this channel.
        /// </summary>
        public Instant? FirstDelivered { get; set; }

        /// <summary>
        /// The first time the notification has been marked as seen for this channel.
        /// </summary>
        public Instant? FirstSeen { get; set; }

        /// <summary>
        /// The first time the notification has been marked as confirmed for this channel.
        /// </summary>
        public Instant? FirstConfirmed { get; set; }

        public static UserNotificationChannelDto FromDomainObject(UserNotificationChannel source)
        {
            var result = SimpleMapper.Map(source, new UserNotificationChannelDto
            {
                Status = new Dictionary<string, ChannelSendInfoDto>()
            });

            if (source.Setting != null)
            {
                result.Setting = ChannelSettingDto.FromDomainObject(source.Setting);
            }
            else
            {
                result.Setting = new ChannelSettingDto();
            }

            if (source.Status != null)
            {
                foreach (var (key, value) in source.Status)
                {
                    result.Status[key] = ChannelSendInfoDto.FromDomainObject(value);
                }
            }

            return result;
        }
    }
}
