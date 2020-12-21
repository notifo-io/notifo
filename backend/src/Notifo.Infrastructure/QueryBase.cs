// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure
{
    public abstract class QueryBase
    {
        public int Skip { get; set; }

        public int Take { get; set; } = 20;
    }
}
