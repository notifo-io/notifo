// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Events.Pipeline
{
    public sealed class EventPipelineOptions
    {
        public string ChannelName { get; set; } = "events";
    }
}
