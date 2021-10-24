// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.Collections;

namespace Notifo.Domain.Channels.WebPush
{
    public sealed record WebPushSubscription
    {
        public string Endpoint { get; init; }

        public ReadonlyDictionary<string, string> Keys { get; init; } = ReadonlyDictionary.Empty<string, string>();
    }
}
