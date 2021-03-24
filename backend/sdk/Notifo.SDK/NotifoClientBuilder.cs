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
    public sealed class NotifoClientBuilder
    {
        private string apiKey;
        private string apiUrl = "https://app.notifo.io";
        private TimeSpan timeout = TimeSpan.FromSeconds(10);
        private HttpClient httpClient;

        private NotifoClientBuilder()
        {
        }

        public static NotifoClientBuilder Create()
        {
            return new NotifoClientBuilder();
        }

        public NotifoClientBuilder SetApiKey(string apiKey)
        {
            this.apiKey = apiKey;

            return this;
        }

        public NotifoClientBuilder SetApiUrl(string apiUrl)
        {
            this.apiUrl = apiUrl;

            return this;
        }

        public NotifoClientBuilder SetTimeout(TimeSpan timeout)
        {
            this.timeout = timeout;

            return this;
        }

        public NotifoClientBuilder SetClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;

            return this;
        }

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
