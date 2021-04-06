// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;

namespace Notifo.SDK
{
    /// <summary>
    /// Fluent builder to create a new instance of the <see cref="INotifoClient"/> class.
    /// </summary>
    public sealed class NotifoClientBuilder
    {
        private string apiKey;
        private string apiUrl = "https://app.notifo.io";
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
        /// Build a new instance of the <see cref="INotifoClient"/> class.
        /// </summary>
        /// <returns>The generated <see cref="INotifoClient"/> instance.</returns>
        /// <exception cref="InvalidOperationException">Configuration is not valid.</exception>
        public INotifoClient Build()
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("API Key not defined.");
            }

            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                throw new InvalidOperationException("API URL not defined.");
            }

            if (!Uri.IsWellFormedUriString(apiUrl, UriKind.Absolute))
            {
                throw new InvalidOperationException("API URL is not a well defined absolute URL.");
            }

            httpClient ??= new HttpClient();
            httpClient.Timeout = timeout;
            httpClient.DefaultRequestHeaders.Add("ApiKey", apiKey);

            var client = new NotifoClient(httpClient, apiUrl);

            return client;
        }
    }
}
