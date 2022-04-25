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
        private readonly Uri? baseUrl;

        public ImageFormatter(IUrlGenerator urlGenerator)
        {
            Uri.TryCreate(urlGenerator.BuildUrl(), UriKind.Absolute, out baseUrl);
        }

        public string Format(string? url, string preset)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                if (baseUrl != null && uri.Host == baseUrl.Host && uri.Port == baseUrl.Port)
                {
                    return $"{url}?preset={preset}";
                }

                return url!;
            }

            return $"{baseUrl}/Empty.Png";
        }
    }
}
