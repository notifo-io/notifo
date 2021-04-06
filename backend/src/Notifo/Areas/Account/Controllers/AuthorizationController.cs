// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notifo.Infrastructure;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Notifo.Areas.Account.Controllers
{
    public class AuthorizationController : ControllerBase<AuthorizationController>
    {
        private readonly IOpenIddictScopeManager scopeManager;

        public AuthorizationController(IOpenIddictScopeManager scopeManager)
        {
            this.scopeManager = scopeManager;
        }

        [HttpGet("connect/authorize")]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.GetOpenIddictServerRequest();

            if (request == null)
            {
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
            }

            if (User.Identity?.IsAuthenticated != true)
            {
                if (request.HasPrompt(Prompts.None))
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                return Challenge();
            }

            var user = await UserService.GetAsync(User);

            if (user == null)
            {
                throw new InvalidOperationException("The user details cannot be retrieved.");
            }

            var principal = await SignInManager.CreateUserPrincipalAsync((IdentityUser)user.Identity);

            var scopes = request.GetScopes();

            principal.SetScopes(request.GetScopes());
            principal.SetResources(await scopeManager.ListResourcesAsync(scopes).ToListAsync());

            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim, principal));
            }

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpGet("connect/logout")]
        public async Task<IActionResult> Logout()
        {
            await SignInManager.SignOutAsync();

            return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private static IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
        {
            switch (claim.Type)
            {
                case Claims.Name:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Profile))
                    {
                        yield return Destinations.IdentityToken;
                    }

                    yield break;

                case Claims.Email:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Email))
                    {
                        yield return Destinations.IdentityToken;
                    }

                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Roles))
                    {
                        yield return Destinations.IdentityToken;
                    }

                    yield break;

                case "AspNet.Identity.SecurityStamp":
                    yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }
    }
}