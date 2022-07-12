// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Infrastructure.Scheduling
{
    public interface IScheduler<in T>
    {
        void Complete(string key);

        Task ScheduleAsync(string key, T job, Instant dueTime, bool canInline,
            CancellationToken ct = default);

        Task ScheduleAsync(string key, T job, Duration dueTimeFromNow, bool canInline,
            CancellationToken ct = default);

        Task ScheduleGroupedAsync(string key, T job, Instant dueTime, bool canInline,
            CancellationToken ct = default);

        Task ScheduleGroupedAsync(string key, T job, Duration dueTimeFromNow, bool canInline,
            CancellationToken ct = default);
    }
}
