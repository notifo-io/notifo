// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Mediator;

#pragma warning disable MA0048 // File name must match type name

public delegate ValueTask<object?> NextDelegate(object request, CancellationToken ct);

public interface IMessageMiddleware<T>
{
    ValueTask<object?> HandleAsync(T request, NextDelegate next,
        CancellationToken ct);
}

public interface IMessageMiddleware
{
    ValueTask<object?> HandleAsync(object request, NextDelegate next,
        CancellationToken ct);
}
