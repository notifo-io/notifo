// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels.WebPush
{
    public sealed class WebPushOptions
    {
        public string VapidPublicKey { get; set; }

        public string VapidPrivateKey { get; set; }

        public string Subject { get; set; }
    }
}
