// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Notifo.Domain.Events
{
    public sealed class EventProperties : Dictionary<string, string>
    {
        public EventProperties()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public EventProperties(IReadOnlyDictionary<string, string> source)
            : base(source, StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
