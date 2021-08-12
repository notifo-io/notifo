// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using NodaTime;

namespace Notifo.Domain.ChannelTemplates
{
    public sealed class ChannelTemplate<T>
    {
        public string Id { get; private init; }

        public string AppId { get; private init; }

        public string? Name { get; set; }

        public bool Primary { get; set; }

        public Instant LastUpdate { get; set; }

        public Dictionary<string, T> Languages { get; set; } = new Dictionary<string, T>();

        public static ChannelTemplate<T> Create(string appId, string id)
        {
            var user = new ChannelTemplate<T> { AppId = appId, Id = id };

            return user;
        }
    }
}
