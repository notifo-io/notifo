// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Mediator;

public interface IMediator
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request,
        CancellationToken ct = default);

    Task PublishAsync<TNotification>(TNotification notification,
        CancellationToken ct = default) where TNotification : notnull;
}
