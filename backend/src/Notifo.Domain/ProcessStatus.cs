// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain
{
    public enum ProcessStatus
    {
        None,
        Attempt,
        Handled,
        Failed,
        Skipped
    }
}
