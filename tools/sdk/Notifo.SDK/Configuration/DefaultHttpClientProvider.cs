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

        public StoredHttpClient(DefaultHttpClientProvider parent, INotifoOptions options)
        {
            this.options = new StaticNotifoOptions(options);

            var authenticator =
                new CachingAuthenticator(new Authenticator(parent,
                    options.ApiKey,
                    options.ClientId,
                    options.ClientSecret));

            HttpClient = options.BuildHttpClient(new AuthenticatingHttpMessageHandler(authenticator));

            if (HttpClient.BaseAddress == null)
            {
                HttpClient.BaseAddress = new Uri(options.ApiUrl);
            }

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
