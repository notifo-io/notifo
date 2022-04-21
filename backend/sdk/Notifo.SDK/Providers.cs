// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.SDK
{
    /// <summary>
    /// Defines all the providers.
    /// </summary>
    public static class Providers
    {
        /// <summary>
        /// The name of the Email provider.
        /// </summary>
        public static readonly string Email = "email";

        /// <summary>
        /// The name of the MobilePush provider.
        /// </summary>
        public static readonly string MobilePush = "mobilepush";

        /// <summary>
        /// The name of the Sms provider.
        /// </summary>
        public static readonly string Sms = "sms";

        /// <summary>
        /// The name of the WebHook provider.
        /// </summary>
        public static readonly string WebHook = "webhook";

        /// <summary>
        /// The name of the WebPush provider.
        /// </summary>
        public static readonly string WebPush = "webpush";
    }
}
