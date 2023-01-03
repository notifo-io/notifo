// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.SDK.Configuration;

/// <summary>
/// Handles authentication tokens.
/// </summary>
public interface IAuthenticator
{
    /// <summary>
    /// Gets the JWT bearer token.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>
    /// The JWT bearer token.
    /// </returns>
    Task<string> GetBearerTokenAsync(
        CancellationToken ct);

    /// <summary>
    /// Removes a token when it has been expired or invalidated.
    /// </summary>
    /// <param name="token">The token to remove.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>
    /// The task for completion.
    /// </returns>
    Task RemoveTokenAsync(string token,
        CancellationToken ct);

    /// <summary>
    /// True, if the request should be intercepted.
    /// </summary>
    /// <param name="request">The request to test.</param>
    /// <returns>
    /// True, if the request should be intercepted from the middleware.
    /// </returns>
    bool ShouldIntercept(HttpRequestMessage request);
}
