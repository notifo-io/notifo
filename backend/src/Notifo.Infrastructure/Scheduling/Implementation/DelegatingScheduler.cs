// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Infrastructure.Scheduling.Implementation;

public sealed class DelegatingScheduler<T> : IScheduler<T>
{
    private readonly IScheduling<T> scheduling;

    public DelegatingScheduler(IScheduling<T> scheduling)
    {
        this.scheduling = scheduling;
    }

    public void Complete(string key)
    {
        scheduling.Complete(key);
    }

    public Task ScheduleAsync(string key, T job, Instant dueTime, bool canInline,
        CancellationToken ct = default)
    {
        return scheduling.ScheduleAsync(key, job, dueTime, canInline, ct);
    }

    public Task ScheduleAsync(string key, T job, Duration dueTimeFromNow, bool canInline,
        CancellationToken ct = default)
    {
        return scheduling.ScheduleAsync(key, job, dueTimeFromNow, canInline, ct);
    }

    public Task ScheduleGroupedAsync(string key, T job, Instant dueTime, bool canInline,
        CancellationToken ct = default)
    {
        return scheduling.ScheduleGroupedAsync(key, job, dueTime, canInline, ct);
    }

    public Task ScheduleGroupedAsync(string key, T job, Duration dueTimeFromNow, bool canInline,
        CancellationToken ct = default)
    {
        return scheduling.ScheduleGroupedAsync(key, job, dueTimeFromNow, canInline, ct);
    }
}
