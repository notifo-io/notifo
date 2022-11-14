// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.Mediator;

namespace Microsoft.Extensions.DependencyInjection;

public static class MediatorServiceExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.AddSingletonAs<DefaultMediator>()
            .As<IMediator>();

        return services;
    }

    public static IServiceCollection AddMiddleware<T>(this IServiceCollection services) where T : class, IMessageMiddleware
    {
        services.AddSingleton<T>();
        services.AddSingleton<IMessageMiddleware, T>();

        return services;
    }

    public static IServiceCollection AddMiddleware<T, TRequest>(this IServiceCollection services) where T : class, IMessageMiddleware<TRequest>
    {
        services.AddSingleton<T>();
        services.AddSingleton<IMessageMiddleware>(
            c => new TypedMessageMiddlewareWrapper<TRequest>(
                c.GetRequiredService<T>()));

        return services;
    }

    public static IServiceCollection AddRequestHandler<T, TRequest, TResponse>(this IServiceCollection services) where TRequest : IRequest<TResponse> where T : IRequestHandler<TRequest, TResponse>
    {
        services.AddSingleton<IMessageMiddleware>(
            c => new TypedRequestHandlerWrapper<TRequest, TResponse>(
                c.GetRequiredService<T>()));

        return services;
    }
}
