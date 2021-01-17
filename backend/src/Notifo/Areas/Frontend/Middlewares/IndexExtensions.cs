// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Http;

namespace Notifo.Areas.Frontend.Middlewares
{
    public static class IndexExtensions
    {
        public static bool IsDemo(this HttpContext context)
        {
            return context.Request.Path.Value?.EndsWith("/demo.html", StringComparison.OrdinalIgnoreCase) == true;
        }

        public static bool IsIndex(this HttpContext context)
        {
            return context.Request.Path.Value?.EndsWith("/index.html", StringComparison.OrdinalIgnoreCase) == true;
        }

        public static bool IsHtmlPath(this HttpContext context)
        {
            return context.Request.Path.Value?.EndsWith(".html", StringComparison.OrdinalIgnoreCase) == true;
        }

        public static bool IsHtml(this HttpContext context)
        {
            return context.Response.ContentType?.ToLower().Contains("text/html") == true;
        }

        public static string AdjustHtml(this string html, HttpContext httpContext)
        {
            var result = html;

            if (httpContext.Request.PathBase.HasValue)
            {
                result = result.Replace("<base href=\"/\">", $"<base href=\"{httpContext.Request.PathBase}/\">");
            }

            return result;
        }
    }
}
