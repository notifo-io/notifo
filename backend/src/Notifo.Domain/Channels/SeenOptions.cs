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
        public string? Channel { get; init; }

        public string? DeviceIdentifier { get; init; }
    }
}
