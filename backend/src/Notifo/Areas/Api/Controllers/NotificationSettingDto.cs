// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Domain;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers
{
    public sealed class NotificationSettingDto
    {
        /// <summary>
        /// True or false to send the notification for the channel.
        /// </summary>
        [Required]
        public NotificationSend Send { get; set; }

        /// <summary>
        /// The delay in seconds.
        /// </summary>
        public int? DelayInSeconds { get; set; }

        /// <summary>
        /// The template if the channel supports it.
        /// </summary>
        public string? Template { get; set; }

        /// <summary>
        /// Additional properties.
        /// </summary>
        public NotificationProperties? Properties { get; set; }

        public static NotificationSettingDto FromDomainObject(NotificationSetting source)
        {
            var result = SimpleMapper.Map(source, new NotificationSettingDto());

            return result;
        }

        public NotificationSetting ToDomainObject()
        {
            var result = SimpleMapper.Map(this, new NotificationSetting());

            return result;
        }
    }
}
