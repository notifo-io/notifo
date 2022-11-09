// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Net.Http.Headers;

namespace Notifo.Pipeline;

public class NotifoMiddleware
{
    private readonly RequestDelegate next;

    public NotifoMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.Equals("/notifo-sw.js", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Headers[HeaderNames.ContentType] = "text/javascript";

            var script = "importScripts('https://app.notifo.io/notifo-sdk-worker.js')";

            await context.Response.WriteAsync(script, context.RequestAborted);
        }
        else
        {
            await next(context);
        }
    }
}
