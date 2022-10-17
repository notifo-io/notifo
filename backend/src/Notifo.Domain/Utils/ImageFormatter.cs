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

        public string Format(string? url, string? preset, bool emptyFallback)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                var baseUrl = urlGenerator.BuildUrl(string.Empty);

                if (url.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
                {
                    var result = $"{url}";

                    if (!string.IsNullOrEmpty(preset))
                    {
                        result += $"?preset={preset}";
                    }

                    return result;
                }
                else
                {
                    var result = $"{urlGenerator.BuildUrl("/api/assets/proxy")}?url={Uri.EscapeDataString(url)}";

                    if (!string.IsNullOrEmpty(preset))
                    {
                        result += $"&preset={preset}";
                    }

                    return result;
                }
            }

            if (emptyFallback)
            {
                return GetEmptyImage();
            }

            return string.Empty;
        }

        public string GetEmptyImage()
        {
            return urlGenerator.BuildUrl("/Empty.png");
        }
    }
}
