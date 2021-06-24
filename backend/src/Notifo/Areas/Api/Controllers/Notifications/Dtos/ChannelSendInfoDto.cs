// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Notifications.Dtos
{
    public sealed class ChannelSendInfoDto
    {
        /// <summary>
        /// The send status.
        /// </summary>
        public ProcessStatus Status { get; set; }

        /// <summary>
        /// The last update.
        /// </summary>
        public Instant LastUpdate { get; set; }

        /// <summary>
        /// The details.
        /// </summary>
        public string? Detail { get; set; }

        public static ChannelSendInfoDto FromDomainObject(ChannelSendInfo source)
        {
            return SimpleMapper.Map(source, new ChannelSendInfoDto());
        }
    }
}
