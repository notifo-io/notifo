// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;

namespace Notifo.Infrastructure
{
    public static class Telemetry
    {
        public static readonly ActivitySource Activities = new ActivitySource("Notifo");
    }
}
