// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Hosting;

#pragma warning disable MA0048 // File name must match type name

namespace Notifo.Infrastructure.Messaging.Implementation
{
    public delegate Task MessageCallback<T>(Envelope<T> envelope,
            CancellationToken ct);

    public interface IMessaging<T> : IMessageProducer<Envelope<T>>, IInitializable
    {
        Task SubscribeAsync(MessageCallback<T> onMessage,
            CancellationToken ct = default);
    }
}
