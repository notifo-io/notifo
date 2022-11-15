// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Infrastructure.Mediator;

public sealed class DefaultMediator : IMediator
{
    private readonly Lazy<NextDelegate> pipeline;

    public DefaultMediator(IServiceProvider services)
    {
        // Initialize the pipeline with the service provider so that request handlers can inject the mediator.
        pipeline = new Lazy<NextDelegate>(() =>
        {
            // Resolves the circular dependencies.
            var reverseMiddlewares = services.GetRequiredService<IEnumerable<IMessageMiddleware>>().Reverse().ToList();

            NextDelegate next = (v, ct) => default;

            foreach (var middleware in reverseMiddlewares)
            {
                next = Create(next, middleware);
            }

            return next;
        });
    }

    private static NextDelegate Create(NextDelegate next, IMessageMiddleware middleware)
    {
        return (c, ct) => middleware.HandleAsync(c, next, ct);
    }

    public async Task PublishAsync<TNotification>(TNotification notification,
        CancellationToken ct = default) where TNotification : notnull
    {
        await pipeline.Value(notification, ct);
    }

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request,
        CancellationToken ct = default)
    {
        var result = await pipeline.Value(request, ct);

        if (Equals(result, default(TResponse)))
        {
            return default!;
        }

        if (result is not TResponse typed)
        {
            throw new InvalidOperationException("No handler returns matching result type.");
        }

        return typed;
    }
}
