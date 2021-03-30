// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels
{
    public struct SeenOptions
    {
        public bool IsOffline { get; init; }

        public string? Channel { get; init; }
    }
}
