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
        public static string? OpenIdSubject(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        public static string? AppId(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(x => x.Type == DefaultClaimTypes.AppId)?.Value;
        }

        public static string? AppName(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(x => x.Type == DefaultClaimTypes.AppName)?.Value;
        }

        public static string? UserId(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(x => x.Type == DefaultClaimTypes.UserId)?.Value;
        }
    }
}
