// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.SDK.Configuration;

internal sealed class DefaultHttpClientProvider : IHttpClientProvider
{
    private readonly INotifoOptions options;
    private StoredHttpClient currentClient;

    private sealed class StoredHttpClient
    {
        private readonly StaticNotifoOptions options;

        public HttpClient HttpClient { get; }

        public StoredHttpClient(IHttpClientProvider parent, INotifoOptions options)
        {
            this.options = new StaticNotifoOptions(options);

            if (!string.IsNullOrWhiteSpace(options.ApiKey))
            {
                HttpClient = new HttpClient();
                HttpClient.DefaultRequestHeaders.Add("ApiKey", options.ApiKey);
            }
            else
            {
                var authenticator = new Authenticator(parent, options.ClientId, options.ClientSecret);

                HttpClient = new HttpClient(new AuthenticatingHttpMessageHandler(new CachingAuthenticator(authenticator)));
            }

            HttpClient.BaseAddress = new Uri(options.ApiUrl);
            HttpClient.Timeout = options.Timeout;
        }

        public bool IsMatch(INotifoOptions options)
        {
            return this.options.Equals(options);
        }
    }

    public DefaultHttpClientProvider(INotifoOptions options)
    {
        this.options = options;
    }

    public HttpClient Get()
    {
        var httpClient = currentClient;

        if (httpClient?.IsMatch(options) != true)
        {
            httpClient = currentClient = new StoredHttpClient(this, options);
        }

        return httpClient.HttpClient;
    }

    public void Return(HttpClient httpClient)
    {
    }
}
