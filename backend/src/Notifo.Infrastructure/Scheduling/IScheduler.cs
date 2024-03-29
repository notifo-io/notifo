﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Infrastructure.Scheduling;

public interface IScheduler<in T>
{
    Task<bool> CompleteAsync(string key,
        CancellationToken ct = default);

    Task<bool> CompleteAsync(string key, string groupKey,
        CancellationToken ct = default);

    Task ScheduleAsync(string key, T job, Instant dueTime, bool canInline,
        CancellationToken ct = default);

    Task ScheduleAsync(string key, T job, Duration dueTimeFromNow, bool canInline,
        CancellationToken ct = default);

    Task ScheduleGroupedAsync(string key, string groupKey, T job, Instant dueTime, bool canInline,
        CancellationToken ct = default);

    Task ScheduleGroupedAsync(string key, string groupKey, T job, Duration dueTimeFromNow, bool canInline,
        CancellationToken ct = default);
}
