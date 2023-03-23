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

    public bool Progressing { get; set; }

    public int RetryCount { get; set; }

    public Instant? ProgressingStarted { get; set; }

    public Instant DueTime { get; set; }

    public Dictionary<string, T>? JobsV2 { get; set; }

    [Obsolete("Jobs are added with their keys now to avoid duplicates")]
    public List<T>? Jobs { get; set; }

    public List<T> GetAllJobs()
    {
        var result = new List<T>();

        if (JobsV2 != null)
        {
            result.AddRange(JobsV2.Values);
        }

#pragma warning disable CS0618 // Type or member is obsolete
        if (Jobs != null)
        {
            result.AddRange(Jobs);
        }
#pragma warning restore CS0618 // Type or member is obsolete

        return result;
    }
}
