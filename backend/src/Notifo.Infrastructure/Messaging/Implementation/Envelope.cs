// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Infrastructure.Messaging.Implementation
{
    public sealed class Envelope<T>
    {
        public Instant Created { get; init; }

        public T Payload { get; init; }

        public static Envelope<T> Create(T payload)
        {
            var created = SystemClock.Instance.GetCurrentInstant();

            return new Envelope<T> { Created = created, Payload = payload };
        }
    }
}
