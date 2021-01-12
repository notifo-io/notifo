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
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Notifo.Domain.Utils;
using Notifo.Infrastructure.Security;

namespace Notifo.Identity
{
    public sealed class ApiKeyGenerator : IIApiJwtTokenGenerator
    {
        private readonly IServiceProvider serviceProvider;
        private readonly int lifetime = (int)TimeSpan.FromDays(365 * 10).TotalSeconds;
        private readonly Client client = new Client
        {
            AlwaysSendClientClaims = true,
            ClientId = "notifo",
            ClientName = "notifo",
            ClientClaimsPrefix = null
        };

        public ApiKeyGenerator(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task<string> GenerateAppTokenAsync(string appId)
        {
            var claims = new[]
            {
                new Claim(JwtClaimTypes.Subject, appId),
                new Claim(DefaultClaimTypes.AppId, appId),
            };

            return CreateTokenAsync(claims);
        }

        public Task<string> GenerateUserTokenAsync(string appId, string userId)
        {
            var claims = new[]
            {
                new Claim(JwtClaimTypes.Subject, userId),
                new Claim(DefaultClaimTypes.AppId, appId),
            };

            return CreateTokenAsync(claims);
        }

        private async Task<string> CreateTokenAsync(IEnumerable<Claim> claims)
        {
            var claimsIdentity = new ClaimsIdentity();

            claimsIdentity.AddClaim(new Claim(JwtClaimTypes.AuthenticationTime, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()));
            claimsIdentity.AddClaim(new Claim(JwtClaimTypes.IdentityProvider, "notifo"));

            foreach (var claim in claims)
            {
                claimsIdentity.AddClaim(claim);
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var resourceStore = scope.ServiceProvider.GetRequiredService<IResourceStore>()!;
                var resources = await resourceStore.GetAllResourcesAsync();

                var request = new TokenCreationRequest
                {
                    Subject = new ClaimsPrincipal(claimsIdentity),
                    ValidatedResources = new ResourceValidationResult(resources),
                    ValidatedRequest = new ValidatedRequest
                    {
                        Client = client,
                        ClientId = client.ClientId,
                        ClientClaims = claims.Where(x => x.Type != JwtClaimTypes.Subject).ToList(),
                        AccessTokenLifetime = lifetime,
                        AccessTokenType = AccessTokenType.Jwt,
                    }
                };

                var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

                var token = await tokenService.CreateAccessTokenAsync(request);

                return await tokenService.CreateSecurityTokenAsync(token);
            }
        }
    }

    public class X : DefaultClaimsService
    {
        public X(IProfileService profile, ILogger<DefaultClaimsService> logger)
            : base(profile, logger)
        {
        }

        public override async Task<IEnumerable<Claim>> GetAccessTokenClaimsAsync(ClaimsPrincipal subject, ResourceValidationResult resourceResult, ValidatedRequest request)
        {
            var s = await base.GetAccessTokenClaimsAsync(subject, resourceResult, request);

            return s;
        }
    }
}
