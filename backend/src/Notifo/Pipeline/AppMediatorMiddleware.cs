// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
            request.Principal = httpContext?.User ?? throw new InvalidOperationException("Cannot resolve Principal.");
            request.PrincipalId = request.Principal.Sub() ?? throw new InvalidOperationException("Cannot resolve Principal ID.");
        }

        if (request.Timestamp == default)
        {
            var clock = httpContext?.RequestServices.GetRequiredService<IClock>() ?? SystemClock.Instance;

            request.Timestamp = clock.GetCurrentInstant();
        }

        return next(request, ct);
    }
}
