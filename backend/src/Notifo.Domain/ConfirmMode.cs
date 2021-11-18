// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain
{
    public enum ConfirmMode
    {
        None,
        Explicit,

        [Obsolete("Not supported across all platforms.")]
        Seen
    }
}
