// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.Extensions.Options;
using Notifo.Infrastructure.Json;

namespace Notifo.Pipeline;

public static class SpaExtensions
{
    public static string AddOptions(this string html, HttpContext httpContext)
    {
        const string Placeholder = "/* INJECT OPTIONS */";

        if (!html.Contains(Placeholder, StringComparison.Ordinal))
        {
            return html;
        }

        var spaOptions = httpContext.RequestServices.GetService<IOptions<SpaOptions>>()?.Value;

        if (spaOptions != null)
        {
            var serializer = httpContext.RequestServices.GetRequiredService<IJsonSerializer>();

            var script = $"var options = {serializer.SerializeToString(spaOptions)}";

            html = html.Replace(Placeholder, script, StringComparison.OrdinalIgnoreCase);
        }

        return html;
    }
}
