﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Infrastructure.Scheduling.Implementation;

public sealed class DelegatingScheduler<T>(IScheduling<T> scheduling) : IScheduler<T>
{
    public Task<bool> CompleteAsync(string key,
        CancellationToken ct = default)
    {
        return scheduling.CompleteAsync(key, ct);
    }

    public Task<bool> CompleteAsync(string key, string groupKey,
        CancellationToken ct = default)
    {
        return scheduling.CompleteAsync(key, groupKey, ct);
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

    public Task ScheduleGroupedAsync(string key, string groupKey, T job, Instant dueTime, bool canInline,
        CancellationToken ct = default)
    {
        return scheduling.ScheduleGroupedAsync(key, groupKey, job, dueTime, canInline, ct);
    }

    public Task ScheduleGroupedAsync(string key, string groupKey, T job, Duration dueTimeFromNow, bool canInline,
        CancellationToken ct = default)
    {
        return scheduling.ScheduleGroupedAsync(key, groupKey, job, dueTimeFromNow, canInline, ct);
    }
}
