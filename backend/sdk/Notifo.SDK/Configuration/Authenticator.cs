// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Notifo.SDK.Configuration
{
    /// <summary>
    /// THe default implementation of the <see cref="IAuthenticator"/> interface that makes POST
    /// requests to retrieve the JWT bearer token from the connect endpoint.
    /// </summary>
    /// <seealso cref="IAuthenticator" />
    public class Authenticator : IAuthenticator
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly string url;
        private readonly string clientId;
        private readonly string clientSecret;

        /// <summary>
        /// Initializes a new instance of the <see cref="Authenticator"/> class with the URL, the client ID and secret.
        /// </summary>
        /// <param name="url">The URL to the endpoint.</param>
        /// <param name="clientId">The client ID.</param>
        /// <param name="clientSecret">The client Secret.</param>
        /// <exception cref="ArgumentNullException"><paramref name="url"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="clientId"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="clientSecret"/> is null.</exception>
        public Authenticator(string url, string clientId, string clientSecret)
        {
            this.url = url ?? throw new ArgumentNullException(nameof(url));
            this.clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            this.clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
        }

        /// <inheritdoc/>
        public Task RemoveTokenAsync(string token)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<string> GetBearerTokenAsync()
        {
            var tokenUrl = $"{url}/connect/token";

            var bodyString = $"grant_type=client_credentials&client_id={clientId}&client_secret={clientSecret}&scope=NotifoAPI";
            var bodyContent = new StringContent(bodyString, Encoding.UTF8, "application/x-www-form-urlencoded");

            using (var response = await httpClient.PostAsync(tokenUrl, bodyContent))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new SecurityException($"Failed to retrieve access token for client '{clientId}', got HTTP {response.StatusCode}.");
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var jsonToken = JToken.Parse(jsonString);

                return jsonToken["access_token"]!.ToString();
            }
        }
    }
}
