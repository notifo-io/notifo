// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Notifo.Infrastructure.Initialization;

namespace Notifo.Infrastructure.Scheduling
{
    public interface IScheduler<T> : IInitializable
    {
        void Subscribe(IScheduleHandler<T> handler);

        Task ScheduleAsync(string key, T job, Instant dueTime, bool canInline, CancellationToken ct = default);

        Task ScheduleDelayedAsync(string key, T job, int delaySeconds, bool canInline, CancellationToken ct = default);
    }
}
