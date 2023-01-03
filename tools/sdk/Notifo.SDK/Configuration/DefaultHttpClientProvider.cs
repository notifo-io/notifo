// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.SDK.Configuration;

/// <summary>
/// The default http client provider.
/// </summary>
public class DefaultHttpClientProvider : IHttpClientProvider
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

            HttpClient = parent.BuildHttpClient(new AuthenticatingHttpMessageHandler(authenticator));

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

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultHttpClientProvider"/> class with the options.
    /// </summary>
    /// <param name="options">The options. Cannot be null.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/>is null.</exception>
    public DefaultHttpClientProvider(INotifoOptions options)
    {
        this.options = options;
    }

    /// <summary>
    /// Builds the HTTP client from the handler.
    /// </summary>
    /// <param name="handler">The client handler.</param>
    /// <returns>
    /// The created HTTP client.
    /// </returns>
    public virtual HttpClient BuildHttpClient(HttpMessageHandler handler)
    {
        return new HttpClient(handler);
    }

    /// <inheritdoc />
    public HttpClient Get()
    {
        var httpClient = currentClient;

        if (httpClient?.IsMatch(options) != true)
        {
            httpClient = currentClient = new StoredHttpClient(this, options);
        }

        return httpClient.HttpClient;
    }

    /// <inheritdoc />
    public void Return(HttpClient httpClient)
    {
    }
}
