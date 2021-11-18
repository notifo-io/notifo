﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;

namespace Notifo.Areas.Frontend.Middlewares
{
    public sealed class IndexMiddleware
    {
        private readonly RequestDelegate next;

        public IndexMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.IsHtmlPath() && context.Response.StatusCode != 304)
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
    }
}
