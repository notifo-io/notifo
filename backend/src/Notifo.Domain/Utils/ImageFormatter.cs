// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Hosting;

namespace Notifo.Domain.Utils
{
    public sealed class ImageFormatter : IImageFormatter
    {
        private readonly IUrlGenerator urlGenerator;

        public ImageFormatter(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        public string? AddProxy(string? url)
        {
            if (url == null || !IsValidUrl(url))
            {
                return null;
            }

            var baseUrl = urlGenerator.BuildUrl();

            if (url.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
            {
                return AppendQuery(url, "emptyOnFailure", "true");
            }

            var proxy = urlGenerator.BuildUrl("/api/assets/proxy", false);

            return AppendQuery(proxy, "url", url);
        }

        public string? AddPreset(string? url, string? preset)
        {
            if (url == null || string.IsNullOrWhiteSpace(preset) || !IsValidUrl(url))
            {
                return url;
            }

            return AppendQuery(url, "preset", preset);
        }

        private static string AppendQuery(string url, string key, string value)
        {
            var separator = '?';

            if (url.Contains('?', StringComparison.Ordinal))
            {
                separator = '&';
            }

            return $"{url}{separator}{key}={Uri.EscapeDataString(value)}";
        }

        private static bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https";
        }
    }
}
