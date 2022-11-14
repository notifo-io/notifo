// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Mediator;

public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    ValueTask<TResponse> HandleAsync(TRequest request,
        CancellationToken ct);
}
