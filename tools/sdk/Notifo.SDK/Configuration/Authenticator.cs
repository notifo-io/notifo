// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security;
using Newtonsoft.Json.Linq;

namespace Notifo.SDK.Configuration;

/// <summary>
/// THe default implementation of the <see cref="IAuthenticator"/> interface that makes POST
/// requests to retrieve the JWT bearer token from the connect endpoint.
/// </summary>
/// <seealso cref="IAuthenticator" />
public class Authenticator : IAuthenticator
{
    private const string TokenUrl = "connect/token";
    private readonly IHttpClientProvider httpClientProvider;
    private readonly string apiKey;
    private readonly string clientId;
    private readonly string clientSecret;

    /// <summary>
    /// Initializes a new instance of the <see cref="Authenticator"/> class.
    /// </summary>
    /// <param name="httpClientProvider">The HTTP client provider.</param>
    /// <param name="apiKey">The API key.</param>
    /// <param name="clientId">The client ID.</param>
    /// <param name="clientSecret">The client secret.</param>
    public Authenticator(IHttpClientProvider httpClientProvider, string apiKey, string clientId, string clientSecret)
    {
        this.httpClientProvider = httpClientProvider;
        this.apiKey = apiKey;
        this.clientId = clientId;
        this.clientSecret = clientSecret;
    }

    /// <inheritdoc/>
    public bool ShouldIntercept(HttpRequestMessage request)
    {
#if NETSTANDARD2_0
        return !request.RequestUri.PathAndQuery.ToLowerInvariant().Contains(TokenUrl);
#else
        return request.RequestUri?.PathAndQuery.Contains(TokenUrl, StringComparison.OrdinalIgnoreCase) != true;
#endif
    }

    /// <inheritdoc/>
    public Task RemoveTokenAsync(AuthToken token,
        CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<AuthToken> GetTokenAsync(
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return new AuthToken("ApiKey", apiKey, Timeout.InfiniteTimeSpan);
        }

        var httpClient = httpClientProvider.Get();
        try
        {
            var httpRequest = BuildRequest();

            using (var response = await httpClient.SendAsync(httpRequest, ct))
            {
                if (!response.IsSuccessStatusCode)
                {
                    var exception = new SecurityException($"Failed to retrieve access token for client '{clientId}', got HTTP {response.StatusCode}.");

                    throw exception;
                }
#if NET5_0_OR_GREATER
                var jsonString = await response.Content.ReadAsStringAsync(ct);
#else
                var jsonString = await response.Content.ReadAsStringAsync();
#endif
                var jsonToken = JToken.Parse(jsonString);

                return new AuthToken("Authorization", $"Bearer {jsonToken["access_token"]}", TimeSpan.FromDays(30));
            }
        }
        finally
        {
            httpClientProvider.Return(httpClient);
        }
    }

    private HttpRequestMessage BuildRequest()
    {
        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["scope"] = "NotifoAPI"
        };

        return new HttpRequestMessage(HttpMethod.Post, TokenUrl)
        {
            Content = new FormUrlEncodedContent(parameters!)
        };
    }
}
