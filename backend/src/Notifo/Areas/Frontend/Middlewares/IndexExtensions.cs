// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Areas.Frontend.Middlewares
{
    public static class IndexExtensions
    {
        public static bool IsHtmlPath(this HttpContext context)
        {
            return context.Request.Path.Value?.EndsWith(".html", StringComparison.OrdinalIgnoreCase) == true;
        }

        public static bool IsIndex(this HttpContext context)
        {
            return
                context.Request.Path == "/" ||
                context.Request.Path == string.Empty ||
                context.Request.Path.StartsWithSegments("/index.html", StringComparison.OrdinalIgnoreCase);
        }

        public static string AdjustHtml(this string html, HttpContext httpContext)
        {
            var result = html;

            if (httpContext.Request.PathBase.HasValue)
            {
                result = result.Replace("<base href=\"/\">", $"<base href=\"{httpContext.Request.PathBase}/\">", StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }
    }
}
