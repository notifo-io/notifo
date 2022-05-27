// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Diagnostics
{
    public sealed class GCHealthCheckOptions
    {
        public long ThresholdInMB { get; set; } = 4 * 1024L;
    }
}
