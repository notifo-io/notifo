// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.SDK.Configuration;

/// <summary>
/// An authenticator that stores the JWT token in the memory cache.
/// </summary>
/// <seealso cref="IAuthenticator" />
public class CachingAuthenticator : IAuthenticator
{
    private readonly IAuthenticator authenticator;
    private DateTimeOffset cacheExpires;
    private AuthToken? cacheEntry;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingAuthenticator"/> class with the cache key,
    /// the memory cache and inner authenticator that does the actual work.
    /// </summary>
    /// <param name="authenticator">The inner authenticator that does the actual work.  Cannot be null.</param>
    /// <exception cref="ArgumentNullException"><paramref name="authenticator"/> is null.</exception>
    public CachingAuthenticator(IAuthenticator authenticator)
    {
        this.authenticator = authenticator;
    }

    /// <inheritdoc/>
    public async Task<AuthToken> GetTokenAsync(
        CancellationToken ct)
    {
        var result = GetFromCache();

        if (result == null)
        {
            result = await authenticator.GetTokenAsync(ct);

            if (result.Expires > TimeSpan.Zero && result.Expires < TimeSpan.MaxValue)
            {
                SetToCache(result);
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public Task RemoveTokenAsync(AuthToken token,
        CancellationToken ct)
    {
        RemoveFromCache();

        return authenticator.RemoveTokenAsync(token, ct);
    }

    /// <inheritdoc/>
    public bool ShouldIntercept(HttpRequestMessage request)
    {
        var shouldIntercept = authenticator.ShouldIntercept(request);

        return shouldIntercept;
    }

    /// <summary>
    /// Gets the current JWT bearer token from the cache.
    /// </summary>
    /// <returns>
    /// The JWT bearer token or null if not found in the cache.
    /// </returns>
    protected AuthToken? GetFromCache()
    {
        if (cacheExpires < DateTimeOffset.UtcNow)
        {
            RemoveFromCache();
        }

        return cacheEntry;
    }

    /// <summary>
    /// Removes from current JWT bearer token from the cache.
    /// </summary>
    protected void RemoveFromCache()
    {
        cacheExpires = default;
        cacheEntry = default;
    }

    /// <summary>
    /// Sets to the current auth token.
    /// </summary>
    /// <param name="token">The auth token.</param>
    protected void SetToCache(AuthToken token)
    {
        cacheExpires = DateTime.UtcNow.Add(token.Expires);
        cacheEntry = token;
    }
}
