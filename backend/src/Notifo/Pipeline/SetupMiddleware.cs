// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Identity;

namespace Notifo.Pipeline;

public sealed class SetupMiddleware
{
    private readonly RequestDelegate next;
    private bool isUserFound;

    public SetupMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {
        if (!IsSpaPath(context) || IsDevServer(context))
        {
            await next(context);
            return;
        }

        if (!isUserFound && await userService.IsEmptyAsync(context.RequestAborted))
        {
            var url = context.Request.PathBase.Add("/account/setup");

            context.Response.Redirect(url);
        }
        else
        {
            isUserFound = true;

            await next(context);
        }
    }

    private static bool IsSpaPath(HttpContext context)
    {
        return context.IsIndex() || !Path.HasExtension(context.Request.Path);
    }

    private static bool IsDevServer(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/ws", StringComparison.OrdinalIgnoreCase);
    }
}
