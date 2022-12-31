// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;
using Squidex.Hosting;

namespace Notifo.Domain.Utils;

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
            return url.AppendQueries("emptyOnFailure", true);
        }

        var proxy = urlGenerator.BuildUrl("/api/assets/proxy", false);

        return proxy.AppendQueries(nameof(url), url);
    }

    public string? AddPreset(string? url, string? preset)
    {
        if (url == null || string.IsNullOrWhiteSpace(preset) || !IsValidUrl(url))
        {
            return url;
        }

        return url.AppendQueries(nameof(preset), preset);
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https";
    }
}
