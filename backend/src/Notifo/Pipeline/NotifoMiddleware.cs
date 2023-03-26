// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Net.Http.Headers;
using Squidex.Hosting;

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
        if (!context.Request.Path.Equals("/notifo-sw.js", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        context.Response.Headers[HeaderNames.ContentType] = "text/javascript";

        var urlGenerator = context.RequestServices.GetRequiredService<IUrlGenerator>();

        var scriptPath = urlGenerator.BuildUrl("notifo-sdk-worker.js", false);
        var scriptImport = $"importScripts('{scriptPath}')";

        await context.Response.WriteAsync(scriptImport, context.RequestAborted);
    }
}
