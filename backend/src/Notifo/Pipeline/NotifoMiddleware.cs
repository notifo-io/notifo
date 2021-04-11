// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Notifo.Pipeline
{
    public class NotifoMiddleware
    {
        private readonly RequestDelegate next;

        public NotifoMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Equals("/notifo-sw.js") ||
                context.Request.Path.Equals("/build/notifo-sw.js"))
            {
                context.Response.Headers[HeaderNames.ContentType] = "text/javascript";

                var script = "importScripts('https://app.notifo.io/build/notifo-sdk-worker.js')";

                await context.Response.WriteAsync(script);
            }
            else
            {
                await next(context);
            }
        }
    }
}
