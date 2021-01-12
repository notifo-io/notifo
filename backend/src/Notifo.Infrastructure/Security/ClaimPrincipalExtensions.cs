// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Security.Claims;

namespace Notifo.Infrastructure.Security
{
    public static class ClaimPrincipalExtensions
    {
        public static string? Sub(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier || x.Type == "sub")?.Value;
        }

        public static string? AppId(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(x => x.Type == DefaultClaimTypes.AppId)?.Value;
        }

        public static string? UserId(this ClaimsPrincipal principal)
        {
            var sub = principal.Sub();

            if (!string.IsNullOrWhiteSpace(sub))
            {
                var appId = principal.AppId();

                if (string.Equals(sub, appId))
                {
                    return null;
                }
            }

            return sub;
        }
    }
}
