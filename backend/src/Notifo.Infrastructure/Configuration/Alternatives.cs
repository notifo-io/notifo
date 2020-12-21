// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Configuration
{
    public sealed class Alternatives : Dictionary<string, Action>
    {
        public Action Default { get; set; }

        public Alternatives()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
