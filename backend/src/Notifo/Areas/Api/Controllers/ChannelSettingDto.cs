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
    public sealed class ChannelSettingDto
    {
        /// <summary>
        /// Defines if to send a notification through this channel.
        /// </summary>
        [Required]
        public ChannelSend Send { get; set; }

        /// <summary>
        /// Defines when to send a notification through this channel.
        /// </summary>
        [Required]
        public ChannelCondition Condition { get; set; }

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

        public static ChannelSettingDto FromDomainObject(ChannelSetting source)
        {
            var result = SimpleMapper.Map(source, new ChannelSettingDto());

            return result;
        }

        public ChannelSetting ToDomainObject()
        {
            var result = SimpleMapper.Map(this, new ChannelSetting());

            return result;
        }
    }
}
