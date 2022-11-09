// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Channels;

namespace Notifo.Domain
{
    public sealed class ChannelSendInfo
    {
        public SendConfiguration? Configuration { get; init; }

        public ProcessStatus Status { get; init; }

        public Instant LastUpdate { get; init; }

        public Instant? FirstConfirmed { get; init; }

        public Instant? FirstSeen { get; init; }

        public Instant? FirstDelivered { get; init; }

        public string? Detail { get; init; }
    }
}
