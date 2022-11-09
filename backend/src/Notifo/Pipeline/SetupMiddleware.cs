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
}
