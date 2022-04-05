// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;

namespace Notifo.Infrastructure
{
    public abstract record QueryBase
    {
        public int Skip { get; set; }

        public int Take { get; set; } = 20;

        public bool TotalNeeded { get; set; } = true;

        public bool ShouldQueryTotal(ICollection result)
        {
            return TotalNeeded && (result.Count >= Take || Skip > 0);
        }
    }
}
