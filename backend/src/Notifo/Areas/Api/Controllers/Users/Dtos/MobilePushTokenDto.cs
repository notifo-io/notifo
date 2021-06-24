// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using NodaTime;
using Notifo.Domain.Channels.MobilePush;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Users.Dtos
{
    public sealed class MobilePushTokenDto
    {
        /// <summary>
        /// The token.
        /// </summary>
        [Required]
        public string Token { get; set; }

        /// <summary>
        /// The device type.
        /// </summary>
        [Required]
        public MobileDeviceType DeviceType { get; set; }

        /// <summary>
        /// The last time the device was woken up.
        /// </summary>
        public Instant? LastWakeup { get; set; }

        public static MobilePushTokenDto FromDomainObject(MobilePushToken source)
        {
            var result = SimpleMapper.Map(source, new MobilePushTokenDto());

            if (result.LastWakeup == default)
            {
                result.LastWakeup = null;
            }

            return result;
        }
    }
}
