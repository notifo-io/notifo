// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.SDK;

/// <summary>
/// Provides access to options.
/// </summary>
public interface INotifoOptions
{
    /// <summary>
    /// Gets the base API Url.
    /// </summary>
    string ApiUrl { get; }

    /// <summary>
    /// Gets the API Key.
    /// </summary>
    string ApiKey { get; }

    /// <summary>
    /// Gets the Client ID.
    /// </summary>
    string ClientId { get; }

    /// <summary>
    /// Gets the Client Secret.
    /// </summary>
    string ClientSecret { get; }

    /// <summary>
    /// Gets the Timeout.
    /// </summary>
    TimeSpan Timeout { get; }

    /// <summary>
    /// Validates the options.
    /// </summary>
    void Validate();

    /// <summary>
    /// Builds the HTTP client from the handler.
    /// </summary>
    /// <param name="handler">The client handler.</param>
    /// <returns>
    /// The created HTTP client.
    /// </returns>
    HttpClient BuildHttpClient(HttpMessageHandler handler)
    {
        return new HttpClient(handler);
    }
}
