// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure
{
    public static class UrlHelper
    {
        public static string? AppendQuery(string? url, string key, string value)
        {
            if (url == null)
            {
                return url;
            }

            if (url.Contains('?', StringComparison.Ordinal))
            {
                return $"{url}&{key}={Uri.EscapeDataString(value)}";
            }
            else
            {
                return $"{url}?{key}={Uri.EscapeDataString(value)}";
            }
        }

        public static string? AppendQueries(string? url, string key1, string? value1, string key2, string? value2)
        {
            if (url == null)
            {
                return url;
            }

            value1 ??= string.Empty;
            value2 ??= string.Empty;

            if (url.Contains('?', StringComparison.Ordinal))
            {
                return $"{url}&{key1}={Uri.EscapeDataString(value1)}&{key2}={Uri.EscapeDataString(value2)}";
            }
            else
            {
                return $"{url}?{key1}={Uri.EscapeDataString(value1)}&{key2}={Uri.EscapeDataString(value2)}";
            }
        }
    }
}
