// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers
{
    public sealed class NotificationSettingDto
    {
        /// <summary>
        /// True or false to send the notification for the channel.
        /// </summary>
        public bool? Send { get; set; }

        /// <summary>
        /// The delay in seconds.
        /// </summary>
        public int? DelayInSeconds { get; set; }

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