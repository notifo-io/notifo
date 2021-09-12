// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;

namespace Notifo.Infrastructure.Messaging.Implementation
{
    public sealed class DelegatingProducer<T> : IMessageProducer<T>
    {
        private readonly IMessaging<T> messaging;
        private readonly string activity = $"Messaging.Produce({typeof(T).Name})";

        public DelegatingProducer(IMessaging<T> messaging)
        {
            this.messaging = messaging;
        }

        public async Task ProduceAsync(string key, T message,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity(activity))
            {
                var envelope = Envelope<T>.Create(message);

                await messaging.ProduceAsync(key, envelope, default);
            }
        }
    }
}
