// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Mediator;

internal sealed class TypedMessageMiddlewareWrapper<T> : IMessageMiddleware
{
    private readonly IMessageMiddleware<T> inner;

    public TypedMessageMiddlewareWrapper(IMessageMiddleware<T> inner)
    {
        this.inner = inner;
    }

    public ValueTask<object?> HandleAsync(object request, NextDelegate next,
        CancellationToken ct)
    {
        if (request is T typed)
        {
            return inner.HandleAsync(typed, next, ct);
        }

        return next(request, ct);
    }
}
