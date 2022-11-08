// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels
{
    public sealed class ChannelProperties : Dictionary<string, string>
    {
        public ChannelProperties()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
