// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Text;

namespace Notifo.Infrastructure;

public static class StringExtensions
{
    public static string BuildFullUrl(this string baseUrl, string path, bool trailingSlash = false)
    {
        Guard.NotNull(path);

        var url = $"{baseUrl.TrimEnd('/')}/{path.Trim('/')}";

        if (trailingSlash &&
            url.IndexOf("#", StringComparison.OrdinalIgnoreCase) < 0 &&
            url.IndexOf("?", StringComparison.OrdinalIgnoreCase) < 0 &&
            url.IndexOf(";", StringComparison.OrdinalIgnoreCase) < 0)
        {
            url += "/";
        }

        return url;
    }

    public static string JoinNonEmpty(string separator, params string?[] parts)
    {
        Guard.NotNull(separator);

        if (parts == null || parts.Length == 0)
        {
            return string.Empty;
        }

        return string.Join(separator, parts.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    public static string OrDefault(this string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        return value;
    }

    public static string OrDefault<T>(this string? value, T fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback?.ToString() ?? string.Empty;
        }

        return value;
    }

    public static string? OrNull(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value;
    }

    public static string Truncate(this string source, int length)
    {
        if (source.Length < length)
        {
            return source;
        }

        var trimLength = length - 3;

        return source[..trimLength] + "...";
    }

    public static string ToIso8601(this DateTime value)
    {
        return value.ToString("yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture);
    }

    public static string ToBase64(this string value)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    }

    public static string FromBase64(this string value)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(value));
    }

    public static string FromOptionalBase64(this string value)
    {
        try
        {
            return Encoding.Default.GetString(Convert.FromBase64String(value));
        }
        catch (FormatException)
        {
            return value;
        }
    }
}
