// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Options;

namespace Notifo.Domain.Utils
{
    public sealed class ImageFormatter : IImageFormatter
    {
        private readonly Uri baseUrl;

        public ImageFormatter(IOptions<ImageFormatterOptions> options)
        {
            baseUrl = options.Value.BaseUrl;
        }

        public string Format(string? url, string preset)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                if (uri.Host == baseUrl.Host && uri.Port == baseUrl.Port)
                {
                    return $"{url}?preset={preset}";
                }

                return url!;
            }

            return $"{baseUrl}/Empty.Png";
        }

        public string? FormatWhenSet(string? url, string preset)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            return Format(url, preset);
        }
    }
}
