// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Messaging
{
    public interface IMessageProducer<in T>
    {
        Task ProduceAsync(string key, T message,
            CancellationToken ct = default);
    }
}
