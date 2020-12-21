// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.UserEvents.Pipeline
{
    public sealed class UserEventPipelineOptions
    {
        public string ChannelName { get; set; } = "user-events";
    }
}
