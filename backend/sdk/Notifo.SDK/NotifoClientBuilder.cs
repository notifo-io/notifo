// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK.Configuration;

namespace Notifo.SDK
{
    /// <summary>
    /// Fluent builder to create a new instance of the <see cref="INotifoClient"/> class.
    /// </summary>
    public sealed class NotifoClientBuilder
    {
        private bool readResponseAsString;
        private string apiKey;
        private string apiUrl = "https://app.notifo.io";
        private string clientId;
        private string clientSecret;
        private TimeSpan timeout = TimeSpan.FromSeconds(10);
        private HttpClient httpClient;

        private NotifoClientBuilder()
        {
        }

        /// <summary>
        /// Create a new builder.
        /// </summary>
        /// <returns>The builder.</returns>
        public static NotifoClientBuilder Create()
        {
            return new NotifoClientBuilder();
        }

        /// <summary>
        /// Sets the api key to use.
        /// </summary>
        /// <param name="apiKey">The api key.</param>
        /// <returns>The current instance.</returns>
        public NotifoClientBuilder SetApiKey(string apiKey)
        {
            this.apiKey = apiKey;

            return this;
        }

        /// <summary>
        /// Sets the client ID to use.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <returns>The current instance.</returns>
        public NotifoClientBuilder SetClientId(string clientId)
        {
            this.clientId = clientId;

            return this;
        }

        /// <summary>
        /// Sets the client Secret to use.
        /// </summary>
        /// <param name="clientSecret">The client Secret.</param>
        /// <returns>The current instance.</returns>
        public NotifoClientBuilder SetClientSecret(string clientSecret)
        {
            this.clientSecret = clientSecret;

            return this;
        }

        /// <summary>
        /// Sets the api URL to use.
        /// </summary>
        /// <param name="apiUrl">The api URL. Default: https://app.notifo.io.</param>
        /// <returns>The current instance.</returns>
        public NotifoClientBuilder SetApiUrl(string apiUrl)
        {
            this.apiUrl = apiUrl;

            return this;
        }

        /// <summary>
        /// Sets the default request timeout.
        /// </summary>
        /// <param name="timeout">The HTTP request timeout. Default: 10 seconds.</param>
        /// <returns>The current instance.</returns>
        public NotifoClientBuilder SetTimeout(TimeSpan timeout)
        {
            this.timeout = timeout;

            return this;
        }

        /// <summary>
        /// Sets a custom HTTP client.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <returns>The current instance.</returns>
        public NotifoClientBuilder SetClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;

            return this;
        }

        /// <summary>
        /// Configures whether the response should be read as string.
        /// </summary>
        /// <param name="readResponseAsString">True, if the response should be read as string.</param>
        /// <returns>The current instance.</returns>
        public NotifoClientBuilder ReadResponseAsString(bool readResponseAsString)
        {
            this.readResponseAsString = readResponseAsString;

            return this;
        }

        /// <summary>
        /// Build a new instance of the <see cref="INotifoClient"/> class.
        /// </summary>
        /// <returns>The generated <see cref="INotifoClient"/> instance.</returns>
        /// <exception cref="InvalidOperationException">Configuration is not valid.</exception>
        public INotifoClient Build()
        {
            if (string.IsNullOrWhiteSpace(apiKey) && string.IsNullOrWhiteSpace(clientId) && string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new InvalidOperationException("Neiter, API Key, nor Client ID and secret is defined.");
            }

            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                throw new InvalidOperationException("API URL not defined.");
            }

            if (!Uri.IsWellFormedUriString(apiUrl, UriKind.Absolute))
            {
                throw new InvalidOperationException("API URL is not a well defined absolute URL.");
            }

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                httpClient ??= new HttpClient();
                httpClient.DefaultRequestHeaders.Add("ApiKey", apiKey);
            }
            else
            {
                httpClient ??= new HttpClient(new AuthenticatingHttpMessageHandler(new CachingAuthenticator(new Authenticator(apiUrl.TrimEnd('/'), clientId, clientSecret))));
            }

            httpClient.Timeout = timeout;

            var client = new NotifoClient(httpClient, apiUrl, readResponseAsString);

            return client;
        }
    }
}
