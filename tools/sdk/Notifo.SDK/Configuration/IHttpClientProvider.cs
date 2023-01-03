// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.SDK.Configuration;

/// <summary>
/// Optional interface to create new <see cref="HttpClient"/> instances.
/// </summary>
/// <remarks>
/// Implement this class if you have custom requirements how the HTTP requests need to be done.
/// </remarks>
public interface IHttpClientProvider
{
    /// <summary>
    /// Creates the HTTP client from the message.
    /// </summary>
    /// <returns>
    /// The HTTP client.
    /// </returns>
    HttpClient Get();

    /// <summary>
    /// Return a HTTP client.
    /// </summary>
    /// <param name="httpClient">The HTTP client to release.</param>
    void Return(HttpClient httpClient);
}
