﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Microsoft.Extensions.Primitives;

namespace Notifo.Areas.Frontend.Middlewares
{
    public sealed class WebpackMiddleware
    {
        private static readonly HashSet<string> MappedEnding = new HashSet<string>
        {
            "/index.html",
            "/notifo-sdk-worker.js",
            "/notifo-sdk.js",
            "/demo.html"
        };

        private readonly RequestDelegate next;

        public WebpackMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (MappedEnding.Contains(context.Request.Path) && context.Response.StatusCode != 304)
            {
                var url = $"https://localhost:3002{context.Request.Path}";

                await ServeWebpackAsync(context, url);
            }
            else if (context.IsHtmlPath() && context.Response.StatusCode != 304)
            {
                var responseBuffer = new MemoryStream();
                var responseBody = context.Response.Body;

                context.Response.Body = responseBuffer;

                await next(context);

                if (context.Response.StatusCode != 304)
                {
                    context.Response.Body = responseBody;

                    var html = Encoding.UTF8.GetString(responseBuffer.ToArray());

                    html = html.AdjustHtml(context);

                    context.Response.ContentLength = Encoding.UTF8.GetByteCount(html);
                    context.Response.Body = responseBody;

                    await context.Response.WriteAsync(html, context.RequestAborted);
                }
            }
            else
            {
                await next(context);
            }
        }

        private static async Task ServeWebpackAsync(HttpContext context, string url)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                using (var client = new HttpClient(handler))
                {
                    var result = await client.GetAsync(url, context.RequestAborted);

                    context.Response.StatusCode = (int)result.StatusCode;

                    if (result.IsSuccessStatusCode)
                    {
                        var text = await result.Content.ReadAsStringAsync(context.RequestAborted);

                        if (result.Content.Headers.ContentType?.MediaType == "text/html")
                        {
                            text = text.AdjustHtml(context);
                        }

                        foreach (var (key, value) in result.Headers)
                        {
                            context.Response.Headers[key] = new StringValues(value.ToArray());
                        }

                        foreach (var (key, value) in result.Content.Headers)
                        {
                            context.Response.Headers[key] = new StringValues(value.ToArray());
                        }

                        await context.Response.WriteAsync(text, context.RequestAborted);
                    }
                }
            }
        }
    }
}
