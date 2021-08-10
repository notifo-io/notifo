// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Notifo.Domain.Channels.WebPush
{
    public sealed class WebPushSubscription
    {
        public string Endpoint { get; set; }

        public Dictionary<string, string> Keys { get; set; } = new Dictionary<string, string>();
    }
}
