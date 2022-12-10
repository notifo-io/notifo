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
    private string cacheEntry;

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
    public async Task<string> GetBearerTokenAsync()
    {
        var result = GetFromCache();

        if (result == null)
        {
            result = await authenticator.GetBearerTokenAsync();

            SetToCache(result, DateTimeOffset.UtcNow.AddDays(50));
        }

        return result;
    }

    /// <inheritdoc/>
    public Task RemoveTokenAsync(string token)
    {
        RemoveFromCache();

        return authenticator.RemoveTokenAsync(token);
    }

    /// <summary>
    /// Gets the current JWT bearer token from the cache.
    /// </summary>
    /// <returns>
    /// The JWT bearer token or null if not found in the cache.
    /// </returns>
    protected string GetFromCache()
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
    /// Sets to the current JWT bearer token.
    /// </summary>
    /// <param name="token">The JWT bearer token.</param>
    /// <param name="expires">The date and time when the token will expire..</param>
    protected void SetToCache(string token, DateTimeOffset expires)
    {
        cacheExpires = expires;
        cacheEntry = token;
    }
}
