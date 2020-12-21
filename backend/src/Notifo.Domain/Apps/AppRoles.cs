// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Notifo.Domain.Apps
{
    public static class AppRoles
    {
        public const string Owner = "Owner";

        public const string Admin = "Admin";

        public const string WebManager = "WebManager";

        public static readonly IReadOnlySet<string> All = new HashSet<string>
        {
            Owner,
            Admin,
            WebManager
        };
    }
}
