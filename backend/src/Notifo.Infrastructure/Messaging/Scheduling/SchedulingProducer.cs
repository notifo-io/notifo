// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure.Scheduling;

namespace Notifo.Infrastructure.Messaging.Scheduling
{
    public sealed class SchedulingProducer<T> : IAbstractProducer<T>
    {
        private readonly IScheduler<T> scheduler;

        public SchedulingProducer(IScheduler<T> scheduler)
        {
            this.scheduler = scheduler;
        }

        public Task InitializeAsync(CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        public Task ProduceAsync(string key, T message)
        {
            return scheduler.ScheduleAsync(key, message, default, false);
        }
    }
}
