// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels
{
    public sealed class ChannelConfiguration : Dictionary<string, string>
    {
        public ChannelConfiguration()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
