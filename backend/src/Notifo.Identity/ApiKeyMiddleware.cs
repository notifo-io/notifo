// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Notifo.Identity
{
    public sealed class ApiKeyMiddleware
    {
        private readonly RequestDelegate next;

        public ApiKeyMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;

            if (!HasAuthHeader(request) && TryGetApiKeyToken(request, out var apiKey))
            {
                request.Headers[HeaderNames.Authorization] = $"Bearer {apiKey}";
            }

            return next(context);
        }

        private static bool TryGetApiKeyToken(HttpRequest request, [MaybeNullWhen(false)] out string apiKey)
        {
            const string ApiKeyPrefix = "ApiKey ";
            const string ApiKeyHeader = "ApiKey";
            const string ApiKeyHeaderX = "X-ApiKey";
            const string AccessTokenQuery = "access_token";

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

        private static bool HasAuthHeader(HttpRequest request)
        {
            const string BearerPrefix = "Bearer ";

            return request.Headers.TryGetValue(HeaderNames.Authorization, out var header) &&
                header.Count > 0 &&
                header.ToString().StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase);
        }
    }
}
