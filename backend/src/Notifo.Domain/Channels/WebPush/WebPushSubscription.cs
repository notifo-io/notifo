// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Notifo.Domain.Channels.WebPush
{
    public sealed class WebPushSubscription : IEquatable<WebPushSubscription>
    {
        public string Endpoint { get; set; }

        public Dictionary<string, string> Keys { get; set; } = new Dictionary<string, string>();

        public override bool Equals(object? obj)
        {
            return Equals(obj as WebPushSubscription);
        }

        public bool Equals(WebPushSubscription? other)
        {
            return string.Equals(other?.Endpoint, Endpoint);
        }

        public override int GetHashCode()
        {
            return Endpoint?.GetHashCode() ?? 0;
        }
    }
}
