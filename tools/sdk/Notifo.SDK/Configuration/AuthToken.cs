// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.SDK.Configuration;

/// <summary>
/// Represents an auth token.
/// </summary>
public sealed class AuthToken
{
    /// <summary>
    /// Gets or sets the name of header.
    /// </summary>
    public string HeaderName { get; }

    /// <summary>
    /// Gets or sets the value of the header.
    /// </summary>
    public string HeaderValue { get; }

    /// <summary>
    /// The expiration.
    /// </summary>
    public TimeSpan Expires { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthToken"/> class.
    /// </summary>
    /// <param name="headerName">The name of the header.</param>
    /// <param name="headerValue">The value of the header.</param>
    /// <param name="expires">The expiration.</param>
    public AuthToken(string headerName, string headerValue, TimeSpan expires)
    {
        HeaderName = headerName;
        HeaderValue = headerValue;
        Expires = expires;
    }
}
