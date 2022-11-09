// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Scheduling;

public sealed class SchedulerOptions
{
    public string QueueName { get; set; }

    public int MaxParallelism { get; set; } = Environment.ProcessorCount;

    public int[] ExecutionRetries { get; set; } = { 5000, 10000, 30000, 60000 };

    public bool ExecuteInline { get; set; } = true;

    public TimeSpan FailedTimeout { get; set; } = TimeSpan.FromMinutes(5);

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(4);
}
