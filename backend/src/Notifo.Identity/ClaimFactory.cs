// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Notifo.Identity
{
    internal sealed class ClaimFactory : UserClaimsPrincipalFactory<NotifoUser>
    {
        public ClaimFactory(UserManager<NotifoUser> userManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        {
        }

        public override async Task<ClaimsPrincipal> CreateAsync(NotifoUser user)
        {
            var principal = await base.CreateAsync(user);

            if (principal.Identity is ClaimsIdentity claimsIdentity)
            {
                claimsIdentity.AddClaim(new Claim(JwtClaimTypes.Subject, user.Id));
            }

            return principal;
        }
    }
}
