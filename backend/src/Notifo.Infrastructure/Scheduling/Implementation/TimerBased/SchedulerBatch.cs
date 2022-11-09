// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Infrastructure.Scheduling.Implementation.TimerBased;

public sealed class SchedulerBatch<T>
{
    public string Id { get; set; }

    public string Key { get; set; }

    public List<T> Jobs { get; set; }

    public bool Progressing { get; set; }

    public int RetryCount { get; set; }

    public Instant? ProgressingStarted { get; set; }

    public Instant DueTime { get; set; }
}
