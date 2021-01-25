// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Notifo.Domain.Identity
{
    public static class NotifoClaimsExtensions
    {
        public static bool HasConsent(this IEnumerable<Claim> user)
        {
            return user.HasClaimValue(NotifoClaimTypes.Consent, "true");
        }

        public static bool HasConsentForEmails(this IEnumerable<Claim> user)
        {
            return user.HasClaimValue(NotifoClaimTypes.ConsentForEmails, "true");
        }

        public static bool HasClaim(this IEnumerable<Claim> user, string type)
        {
            return user.GetClaims(type).Any();
        }

        public static bool HasClaimValue(this IEnumerable<Claim> user, string type, string value)
        {
            return user.GetClaims(type).Any(x => string.Equals(x.Value, value, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<Claim> GetClaims(this IEnumerable<Claim> user, string request)
        {
            foreach (var claim in user)
            {
                if (claim.Type.Equals(request, StringComparison.OrdinalIgnoreCase))
                {
                    yield return claim;
                }
            }
        }
    }
}
