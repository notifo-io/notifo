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

namespace Notifo.Infrastructure.Scheduling
{
    public interface IScheduler<T> : IInitializable
    {
        void Complete(string key);

        void Subscribe(IScheduleHandler<T> handler);

        Task ScheduleAsync(string key, T job, Instant dueTime, bool canInline, CancellationToken ct = default);

        Task ScheduleAsync(string key, T job, Duration dueTimeFromNow, bool canInline, CancellationToken ct = default);

        Task ScheduleGroupedAsync(string key, T job, Instant dueTime, bool canInline, CancellationToken ct = default);

        Task ScheduleGroupedAsync(string key, T job, Duration dueTimeFromNow, bool canInline, CancellationToken ct = default);
    }
}
