// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Notifo.Infrastructure.Scheduling;
using Notifo.Infrastructure.Scheduling.Implementation;

namespace Notifo.Infrastructure.Messaging.Implementation.Scheduling
{
    public sealed class SchedulingMessaging<T> : IMessaging<T>, IScheduleHandler<Envelope<T>>
    {
        private readonly IScheduling<Envelope<T>> scheduling;
        private readonly DelegatingScheduleHandler<Envelope<T>> handler;
        private MessageCallback<T>? onMessage;

        public int Order => 5000;

        public SchedulingMessaging(IScheduling<Envelope<T>> scheduling)
        {
            this.scheduling = scheduling;

            handler = new DelegatingScheduleHandler<Envelope<T>>(scheduling, new IScheduleHandler<Envelope<T>>[] { this });
        }

        public Task InitializeAsync(CancellationToken ct)
        {
            return handler.InitializeAsync(ct);
        }

        public Task ReleaseAsync(CancellationToken ct)
        {
            return handler.ReleaseAsync(ct);
        }

        public Task ProduceAsync(string key, Envelope<T> envelope,
            CancellationToken ct = default)
        {
            return scheduling.ScheduleAsync(key, envelope, default(Instant), false, ct);
        }

        public Task SubscribeAsync(MessageCallback<T> onMessage,
            CancellationToken ct = default)
        {
            this.onMessage = onMessage;

            return Task.CompletedTask;
        }

        public async Task<bool> HandleAsync(Envelope<T> job, bool isLastAttempt, CancellationToken ct)
        {
            if (onMessage != null)
            {
                await onMessage(job, ct);
            }

            return true;
        }
    }
}
