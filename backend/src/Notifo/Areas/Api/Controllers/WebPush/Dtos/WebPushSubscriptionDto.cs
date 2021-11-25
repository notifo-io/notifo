// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.WebPush;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.WebPush.Dtos
{
    public sealed class WebPushSubscriptionDto
    {
        public string Endpoint { get; set; }

        public ReadonlyDictionary<string, string> Keys { get; set; }

        public WebPushSubscription ToSubscription()
        {
            return SimpleMapper.Map(this, new WebPushSubscription());
        }
    }
}
