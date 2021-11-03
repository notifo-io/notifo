// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Notifo.Domain.Apps;
using Notifo.Domain.Users;
using Notifo.Infrastructure.Security;

namespace Notifo.Identity.ApiKey
{
    public sealed class ApiKeyHandler : AuthenticationHandler<ApiKeyOptions>
    {
        private const string ApiKeyPrefix = "ApiKey ";
        private const string ApiKeyHeader = "ApiKey";
        private const string ApiKeyHeaderX = "X-ApiKey";
        private const string AccessTokenQuery = "access_token";
        private readonly IAppStore appStore;
        private readonly IUserStore userStore;

        public ApiKeyHandler(IOptionsMonitor<ApiKeyOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IAppStore appStore, IUserStore userStore)
            : base(options, logger, encoder, clock)
        {
            this.appStore = appStore;

            this.userStore = userStore;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                if (!IsApiKey(Request, out var apiKey))
                {
                    return AuthenticateResult.NoResult();
                }

                var app = await appStore.GetByApiKeyAsync(apiKey, Context.RequestAborted);

                if (app != null && app.ApiKeys.TryGetValue(apiKey, out var role))
                {
                    var identity = new ClaimsIdentity(new[]
                    {
                        new Claim(DefaultClaimTypes.AppId, app.Id),
                        new Claim(DefaultClaimTypes.AppName, app.Name),
                        new Claim(DefaultClaimTypes.AppRole, role)
                    }, ApiKeyDefaults.AuthenticationScheme);

                    return Success(identity);
                }

                var user = await userStore.GetByApiKeyAsync(apiKey, Context.RequestAborted);

                if (user != null)
                {
                    var identity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UniqueId),
                        new Claim(DefaultClaimTypes.AppId, user.AppId),
                        new Claim(DefaultClaimTypes.AppName, user.AppId),
                        new Claim(DefaultClaimTypes.UserId, user.Id)
                    }, ApiKeyDefaults.AuthenticationScheme);

                    return Success(identity);
                }

                return AuthenticateResult.Fail("Invalid API Key");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while handling api key.");

                throw;
            }
        }

        private AuthenticateResult Success(ClaimsIdentity identity)
        {
            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        public static bool IsApiKey(HttpRequest request, [MaybeNullWhen(false)] out string apiKey)
        {
            apiKey = null!;

            string authorizationHeader = request.Headers[HeaderNames.Authorization];

            if (authorizationHeader?.StartsWith(ApiKeyPrefix, StringComparison.OrdinalIgnoreCase) == true)
            {
                var key = authorizationHeader[ApiKeyPrefix.Length..].Trim();

                if (!string.IsNullOrWhiteSpace(key))
                {
                    apiKey = key;

                    return true;
                }
            }

            string apiKeyHeader1 = request.Headers[ApiKeyHeader];

            if (!string.IsNullOrWhiteSpace(apiKeyHeader1))
            {
                apiKey = apiKeyHeader1;

                return true;
            }

            string apiKeyHeader2 = request.Headers[ApiKeyHeaderX];

            if (!string.IsNullOrWhiteSpace(apiKeyHeader2))
            {
                apiKey = apiKeyHeader2;

                return true;
            }

            string tokenQuery = request.Query[AccessTokenQuery];

            if (!string.IsNullOrWhiteSpace(tokenQuery))
            {
                apiKey = tokenQuery;

                return true;
            }

            return false;
        }
    }
}
