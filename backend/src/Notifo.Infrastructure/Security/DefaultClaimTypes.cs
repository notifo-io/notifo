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
        public static readonly string AppId = "appId";

        public static readonly string AppName = "appName";

        public static readonly string ApiKeyRole = "apiKeyRole";

        public static readonly string UserId = "userId";

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

        public static string? GetApiKeyRole(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ApiKeyRole)?.Value;
        }
    }
}
