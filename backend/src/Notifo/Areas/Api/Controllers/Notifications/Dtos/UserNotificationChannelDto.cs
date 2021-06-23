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
    public sealed class UserNotificationChannelDto
    {
        /// <summary>
        /// The notification settings.
        /// </summary>
        [Required]
        public NotificationSettingDto Setting { get; set; }

        /// <summary>
        /// The status per token or configuration.
        /// </summary>
        [Required]
        public Dictionary<string, ChannelSendInfoDto> Status { get; set; }

        public static UserNotificationChannelDto FromDomainObject(UserNotificationChannel source)
        {
            var result = new UserNotificationChannelDto
            {
                Status = new Dictionary<string, ChannelSendInfoDto>()
            };

            if (source.Setting != null)
            {
                result.Setting = NotificationSettingDto.FromDomainObject(source.Setting);
            }
            else
            {
                result.Setting = new NotificationSettingDto();
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
