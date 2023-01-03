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
    /// Gets the authentication token.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>
    /// The authentication token.
    /// </returns>
    Task<AuthToken> GetTokenAsync(
        CancellationToken ct);

    /// <summary>
    /// Removes a token when it has been expired or invalidated.
    /// </summary>
    /// <param name="token">The token to remove.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>
    /// The task for completion.
    /// </returns>
    Task RemoveTokenAsync(AuthToken token,
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
