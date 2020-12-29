// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Squidex.Hosting;

namespace Notifo.Infrastructure.Scheduling.TimerBased
{
    public interface ISchedulerStore<T> : IInitializable
    {
        Task CompleteAsync(string id);

        Task<SchedulerBatch<T>?> DequeueAsync(Instant time);

        Task EnqueueScheduledAsync(string key, T job, Instant dueTime, int retryCount = 0, CancellationToken ct = default);

        Task EnqueueWithDelayAsync(string key, T job, Instant delay, int retryCount = 0, CancellationToken ct = default);

        Task ResetDeadAsync(Instant oldTime, Instant next);

        Task RetryAsync(string id, Instant next);
    }
}