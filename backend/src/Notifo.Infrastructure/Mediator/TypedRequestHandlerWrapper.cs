// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Mediator;

internal sealed class TypedRequestHandlerWrapper<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> inner) : IMessageMiddleware where TRequest : IRequest<TResponse>
{
    public async ValueTask<object?> HandleAsync(object request, NextDelegate next,
        CancellationToken ct)
    {
        if (request is TRequest typed)
        {
            return await inner.HandleAsync(typed, ct);
        }

        return await next(request, ct);
    }
}
