// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.SDK;

/// <summary>
/// Provides static options.
/// </summary>
public sealed class StaticNotifoOptions : INotifoOptions, IEquatable<INotifoOptions>
{
    /// <inheritdoc />
    public string ApiUrl { get; set; } = "https://app.notifo.io";

    /// <inheritdoc />
    public string ApiKey { get; set; }

    /// <inheritdoc />
    public string ClientId { get; set; }

    /// <inheritdoc />
    public string ClientSecret { get; set; }

    /// <inheritdoc />
    public TimeSpan Timeout { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StaticNotifoOptions"/> class.
    /// </summary>
    public StaticNotifoOptions()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StaticNotifoOptions"/> class from another options.
    /// </summary>
    /// <param name="source">The source options.</param>
    public StaticNotifoOptions(INotifoOptions source)
    {
        ApiUrl = source.ApiUrl;
        ApiKey = source.ApiKey;
        ClientId = source.ClientId;
        ClientSecret = source.ClientSecret;
        Timeout = source.Timeout;
    }

    /// <inheritdoc />
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey) && string.IsNullOrWhiteSpace(ClientId) && string.IsNullOrWhiteSpace(ClientSecret))
        {
            throw new InvalidOperationException("Neiter, API Key, nor Client ID and secret is defined.");
        }

        if (string.IsNullOrWhiteSpace(ApiUrl))
        {
            throw new InvalidOperationException("API URL not defined.");
        }

        if (!Uri.IsWellFormedUriString(ApiUrl, UriKind.Absolute))
        {
            throw new InvalidOperationException("API URL is not a well defined absolute URL.");
        }
    }

    /// <inheritdoc />
    public bool Equals(INotifoOptions other)
    {
        if (other == null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return
            ApiUrl == other.ApiUrl &&
            ApiKey == other.ApiKey &&
            ClientId == other.ClientId &&
            ClientSecret == other.ClientSecret &&
            Timeout == other.Timeout;
    }
}
