// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Domain.Channels.WebPush;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Users.Dtos
{
    public sealed class WebPushSubscriptionDto
    {
        /// <summary>
        /// The endpoint.
        /// </summary>
        [Required]
        public string Endpoint { get; set; }

        public static WebPushSubscriptionDto FromDomainObject(WebPushSubscription source)
        {
            return SimpleMapper.Map(source, new WebPushSubscriptionDto());
        }
    }
}
