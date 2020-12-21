// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Messaging.GooglePubSub
{
    public sealed class GooglePubSubOptions
    {
        public string ProjectId { get; set; }

        public string Prefix { get; set; }
    }
}
