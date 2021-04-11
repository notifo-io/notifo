// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Notifo.Domain.Integrations
{
    public sealed class IntegrationProperties : Dictionary<string, string>
    {
        public IntegrationProperties()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
