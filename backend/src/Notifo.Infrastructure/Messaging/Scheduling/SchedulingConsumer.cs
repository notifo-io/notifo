// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure.Scheduling;
using Squidex.Hosting;
using Squidex.Log;

namespace Notifo.Infrastructure.Messaging.Scheduling
{
    public sealed class SchedulingConsumer<TConsumer, T> : IScheduleHandler<T>, IInitializable where TConsumer : IAbstractConsumer<T>
    {
        private readonly TConsumer consumer;
        private readonly ISemanticLog log;
        private readonly IScheduler<T> scheduler;

        public SchedulingConsumer(IScheduler<T> scheduler, TConsumer consumer,
            ISemanticLog log)
        {
            this.consumer = consumer;

            this.scheduler = scheduler;
            this.scheduler.Subscribe(this);

            this.log = log;
        }

        public Task InitializeAsync(CancellationToken ct)
        {
            return scheduler.InitializeAsync(ct);
        }

        public Task ReleaseAsync(CancellationToken ct)
        {
            return scheduler.ReleaseAsync(ct);
        }

        public async Task<bool> HandleAsync(List<T> jobs, bool isLastAttempt,
            CancellationToken ct)
        {
            foreach (var job in jobs)
            {
                try
                {
                    await consumer.HandleAsync(job, ct);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, w => w
                        .WriteProperty("action", "ConsumeMessage")
                        .WriteProperty("system", "SchedulingConsumer")
                        .WriteProperty("status", "Failed"));
                }
            }

            return true;
        }
    }
}
