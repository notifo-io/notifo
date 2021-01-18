// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;

namespace Notifo.Infrastructure.Security
{
    public static class DefaultClaimTypes
    {
        public static readonly string AppId = "app_id";

        public static readonly string AppName = "app_name";

        public static readonly string AppRole = "app_role";

        public static readonly string UserId = "user_id";

        public static string? GetAppId(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(AppId)?.Value;
        }

        public static string? GetAppName(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(AppName)?.Value;
        }

        public static string? GetUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(UserId)?.Value;
        }
    }
}
