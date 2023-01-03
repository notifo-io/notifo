// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK.Configuration;

namespace Notifo.SDK;

/// <summary>
/// Fluent builder to create a new instance of the <see cref="INotifoClient"/> class.
/// </summary>
public sealed class NotifoClientBuilder
{
    private bool readResponseAsString;
    private IHttpClientProvider httpClientProvider;
    private INotifoOptions options = new StaticNotifoOptions();

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
    /// Sets the options.
    /// </summary>
    /// <param name="options">The options to use.</param>
    /// <returns>The current instance.</returns>
    public NotifoClientBuilder SetOptions(INotifoOptions options)
    {
        this.options = options;

        return this;
    }

    /// <summary>
    /// Sets the HTTP client provider.
    /// </summary>
    /// <param name="httpClientProvider">The client provider to use.</param>
    /// <returns>The current instance.</returns>
    public NotifoClientBuilder SetHttpClientProvider(IHttpClientProvider httpClientProvider)
    {
        this.httpClientProvider = httpClientProvider;

        return this;
    }

    /// <summary>
    /// Sets the API key to use.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <returns>The current instance.</returns>
    public NotifoClientBuilder SetApiKey(string apiKey)
    {
        if (options is StaticNotifoOptions staticOptions)
        {
            staticOptions.ApiKey = apiKey;
        }

        return this;
    }

    /// <summary>
    /// Sets the client ID to use.
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <returns>The current instance.</returns>
    public NotifoClientBuilder SetClientId(string clientId)
    {
        if (options is StaticNotifoOptions staticOptions)
        {
            staticOptions.ClientId = clientId;
        }

        return this;
    }

    /// <summary>
    /// Sets the client Secret to use.
    /// </summary>
    /// <param name="clientSecret">The client Secret.</param>
    /// <returns>The current instance.</returns>
    public NotifoClientBuilder SetClientSecret(string clientSecret)
    {
        if (options is StaticNotifoOptions staticOptions)
        {
            staticOptions.ClientSecret = clientSecret;
        }

        return this;
    }

    /// <summary>
    /// Sets the API URL to use.
    /// </summary>
    /// <param name="apiUrl">The API URL.</param>
    /// <returns>The current instance.</returns>
    public NotifoClientBuilder SetApiUrl(string apiUrl)
    {
        if (options is StaticNotifoOptions staticOptions)
        {
            staticOptions.ApiUrl = apiUrl;
        }

        return this;
    }

    /// <summary>
    /// Sets the default request timeout.
    /// </summary>
    /// <param name="timeout">The HTTP request timeout. Default: 10 seconds.</param>
    /// <returns>The current instance.</returns>
    public NotifoClientBuilder SetTimeout(TimeSpan timeout)
    {
        if (options is StaticNotifoOptions staticOptions)
        {
            staticOptions.Timeout = timeout;
        }

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
        if (httpClientProvider == null)
        {
            if (options == null)
            {
                throw new InvalidOperationException("Options are not defined.");
            }

            httpClientProvider = new DefaultHttpClientProvider(options);
        }

        var client = new NotifoClient(httpClientProvider, readResponseAsString);

        return client;
    }
}
