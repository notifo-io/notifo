// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels
{
    public sealed class SendConfiguration : Dictionary<string, string>
    {
        public SendConfiguration()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
