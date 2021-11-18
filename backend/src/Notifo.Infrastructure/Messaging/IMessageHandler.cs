// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Messaging
{
    public interface IMessageHandler<in T>
    {
        Task HandleAsync(T message,
            CancellationToken ct = default);
    }
}
