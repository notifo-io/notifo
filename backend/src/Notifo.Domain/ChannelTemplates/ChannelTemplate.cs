// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Infrastructure.Collections;

namespace Notifo.Domain.ChannelTemplates
{
    public sealed record ChannelTemplate<T>
    {
        public string Id { get; private init; }

        public string AppId { get; private init; }

        public string? Name { get; init; }

        public string? Kind { get; init; }

        public bool Primary { get; init; }

        public Instant Created { get; init; }

        public Instant LastUpdate { get; init; }

        public ReadonlyDictionary<string, T> Languages { get; init; } = ReadonlyDictionary.Empty<string, T>();

        public static ChannelTemplate<T> Create(string appId, string id, Instant now)
        {
            return new ChannelTemplate<T> { AppId = appId, Id = id, Created = now };
        }
    }
}
