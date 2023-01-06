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
        private readonly DelegatingHandler httpMessageHandler;

        public StoredHttpClient(DefaultHttpClientProvider parent, INotifoOptions options)
        {
            this.options = new StaticNotifoOptions(options);

            var authenticator =
                new CachingAuthenticator(
                    new Authenticator(parent,
                        options.ApiKey,
                        options.ClientId,
                        options.ClientSecret));

            httpMessageHandler = options.Configure(new AuthenticatingHttpMessageHandler(authenticator));
            httpMessageHandler.InnerHandler ??= new HttpClientHandler();
        }

        public HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient(httpMessageHandler, false);

            if (httpClient.BaseAddress == null)
            {
                httpClient.BaseAddress = new Uri(options.ApiUrl);
            }

            if (options.Timeout > TimeSpan.Zero && options.Timeout < TimeSpan.MaxValue)
            {
                httpClient.Timeout = options.Timeout;
            }

            ((INotifoOptions)options).Configure(httpClient);

            return httpClient;
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
        var storedClient = currentClient;

        if (storedClient?.IsMatch(options) != true)
        {
            storedClient = currentClient = new StoredHttpClient(this, options);
        }

        return storedClient.CreateHttpClient();
    }

    public void Return(HttpClient httpClient)
    {
    }
}
