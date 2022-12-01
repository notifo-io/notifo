// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using NodaTime;
using Notifo.Domain;
using Notifo.Infrastructure.Mediator;
using Notifo.Infrastructure.Security;

namespace Notifo.Pipeline;

public sealed class AppMediatorMiddleware : IMessageMiddleware<AppCommandBase>
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public AppMediatorMiddleware(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public ValueTask<object?> HandleAsync(AppCommandBase request, NextDelegate next,
        CancellationToken ct)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (string.IsNullOrEmpty(request.AppId))
        {
            request.AppId = httpContext?.Features.Get<IAppFeature>()?.App?.Id!;
        }

        if (request.Principal == null)
        {
            request.Principal = GetPrincipal(httpContext);
            request.PrincipalId = GetPrincipalId(httpContext);
        }

        if (request.Timestamp == default)
        {
            var clock = httpContext?.RequestServices.GetRequiredService<IClock>() ?? SystemClock.Instance;

            request.Timestamp = clock.GetCurrentInstant();
        }

        return next(request, ct);
    }

    private static ClaimsPrincipal GetPrincipal(HttpContext? httpContext)
    {
        var user = httpContext?.User;

        return user ?? throw new InvalidOperationException("Cannot resolve Principal.");
    }

    private static string GetPrincipalId(HttpContext? httpContext)
    {
        var id = httpContext?.User?.Sub() ?? httpContext?.User.AppId();

        return id ?? throw new InvalidOperationException("Cannot resolve Principal ID.");
    }
}
