// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Counters;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Texts;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Topics
{
    public sealed record Topic(string AppId, string Path, Instant Created)
    {
        public Instant LastUpdate { get; init; }

        public bool IsExplicit { get; init; }

        public bool ShowAutomatically { get; init; }

        public LocalizedText Name { get; init; } = new LocalizedText();

        public LocalizedText Description { get; init; } = new LocalizedText();

        public ReadonlyDictionary<string, TopicChannel> Channels { get; init; } = ReadonlyDictionary.Empty<string, TopicChannel>();

        public CounterMap Counters { get; init; } = new CounterMap();
    }
}
