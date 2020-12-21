// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.Linq;

namespace Notifo.Infrastructure
{
    public static class StringExtensions
    {
        public static string BuildFullUrl(this string baseUrl, string path, bool trailingSlash = false)
        {
            Guard.NotNull(path, nameof(path));

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
            Guard.NotNull(separator, nameof(separator));

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

        public static string ToIso8601(this DateTime value)
        {
            return value.ToString("yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture);
        }
    }
}
