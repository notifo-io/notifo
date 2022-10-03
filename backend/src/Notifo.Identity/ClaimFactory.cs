// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Notifo.Identity
{
    internal sealed class ClaimFactory : UserClaimsPrincipalFactory<IdentityUser>
    {
        public ClaimFactory(UserManager<IdentityUser> userManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        {
        }

        public override async Task<ClaimsPrincipal> CreateAsync(IdentityUser user)
        {
            var principal = await base.CreateAsync(user);

            if (principal.Identity is ClaimsIdentity claimsIdentity)
            {
                var roles = await UserManager.GetRolesAsync(user);

                foreach (var role in roles)
                {
                    if (!claimsIdentity.Claims.Any(x => x.Type == Claims.Role && x.Value == role))
                    {
                        claimsIdentity.AddClaim(new Claim(Claims.Role, role));
                    }
                }
            }

            return principal;
        }
    }
}
