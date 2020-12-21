// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Counters;

namespace Notifo.Domain.Topics
{
    public sealed class Topic
    {
        public string AppId { get; set; }

        public string Path { get; set; }

        public Instant LastUpdate { get; set; }

        public CounterMap Counters { get; set; }
    }
}
