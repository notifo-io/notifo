// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using NodaTime;
using Notifo.Infrastructure.Timers;

namespace Notifo.Infrastructure.Scheduling.Implementation.TimerBased;

public sealed class TimerConsumer<T>
{
    private readonly ISchedulerStore<T> schedulerStore;
    private readonly SchedulerOptions schedulerOptions;
    private readonly ScheduleSuccessCallback<T> onSuccess;
    private readonly ScheduleErrorCallback<T> onError;
    private readonly ActionBlock<SchedulerBatch<T>> actionBlock;
    private readonly IClock clock;
    private readonly ILogger log;
    private readonly string activity;
    private CompletionTimer? timer;
    private Instant lastReset;

    public TimerConsumer(ISchedulerStore<T> schedulerStore, SchedulerOptions schedulerOptions,
        ScheduleSuccessCallback<T> onSuccess,
        ScheduleErrorCallback<T> onError,
        ILogger log, IClock clock)
    {
        this.schedulerStore = schedulerStore;
        this.schedulerOptions = schedulerOptions;

        this.onSuccess = onSuccess;
        this.onError = onError;

        activity = $"Scheduler.Query({schedulerOptions.QueueName})";

        actionBlock = new ActionBlock<SchedulerBatch<T>>(async batch =>
        {
            using (Telemetry.Activities.StartActivity(activity))
            {
                // An exception would fault the block and no further job would be handled until restart.
                try
                {
                    await HandleAsync(batch);
                }
                catch (Exception ex)
                {
                    if (ex is not OperationCanceledException)
                    {
                        log.LogError(ex, "Failed to handle job.");
                    }
                }
            }
        }, new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = schedulerOptions.MaxParallelism * 4,
            MaxDegreeOfParallelism = schedulerOptions.MaxParallelism,
            MaxMessagesPerTask = 1
        });

        this.clock = clock;

        this.log = log;
    }

    public void Subscribe()
    {
        timer = new CompletionTimer(500, QueryAsync);
    }

    public async Task StopAsync()
    {
        // Stop querying first, otherwise dequeued jobs would be rejected by the completed block.
        if (timer != null)
        {
            await timer.StopAsync();
        }

        actionBlock.Complete();

        await actionBlock.Completion;
    }

    public async Task ExecuteInlineAsync(string key, string? groupKey, T job)
    {
        var retries = schedulerOptions.ExecutionRetries;

        try
        {
            await onSuccess([job], retries.Length == 0, default);
        }
        catch (Exception ex)
        {
            if (ex is not OperationCanceledException)
            {
                log.LogError(ex, "Failed to handle job.");
            }

            if (retries.Length == 0)
            {
                await OnErrorAsync([job], ex);
                return;
            }

            // The inline execution was the first attempt, therefore we continue with the first retry.
            var nextTime = clock.GetCurrentInstant().Plus(Duration.FromMilliseconds(retries[0]));

            if (groupKey != null)
            {
                await schedulerStore.EnqueueGroupedAsync(key, groupKey, job, nextTime, 1);
            }
            else
            {
                await schedulerStore.EnqueueAsync(key, job, nextTime, 1);
            }
        }
    }

    private async Task HandleAsync(SchedulerBatch<T> document)
    {
        // The retry count is also increased when the execution has been interrupted, e.g. by a crash.
        if (document.RetryCount > schedulerOptions.ExecutionRetries.Length)
        {
            await OnErrorAsync(document.GetAllJobs(), new InvalidOperationException("Job has exceeded the maximum number of attempts."));
            await schedulerStore.CompleteAsync(document.Id);
            return;
        }

        var canRetry = document.RetryCount < schedulerOptions.ExecutionRetries.Length;

        bool isConfirmed;
        try
        {
            if (Debugger.IsAttached)
            {
                isConfirmed = await onSuccess(document.GetAllJobs(), !canRetry, default);
            }
            else
            {
                using (var timeout = new CancellationTokenSource(schedulerOptions.Timeout))
                {
                    isConfirmed = await onSuccess(document.GetAllJobs(), !canRetry, timeout.Token);
                }
            }
        }
        catch (Exception ex)
        {
            if (ex is not OperationCanceledException)
            {
                log.LogError(ex, "Failed to handle job.");
            }

            if (canRetry)
            {
                var wait = Duration.FromMilliseconds(schedulerOptions.ExecutionRetries[document.RetryCount]);

                // Use the current time, otherwise delayed jobs would use all retries immediately.
                var nextTime = clock.GetCurrentInstant().Plus(wait);

                await schedulerStore.RetryAsync(document.Id, nextTime);
            }
            else
            {
                await OnErrorAsync(document.GetAllJobs(), ex);
                await schedulerStore.CompleteAsync(document.Id);
            }

            return;
        }

        if (isConfirmed)
        {
            await schedulerStore.CompleteAsync(document.Id);
        }
    }

    private async Task OnErrorAsync(List<T> jobs, Exception exception)
    {
        try
        {
            await onError(jobs, exception, default);
        }
        catch (Exception ex)
        {
            if (ex is not OperationCanceledException)
            {
                log.LogError(ex, "Failed to handle job.");
            }
        }
    }

    public async Task QueryAsync(
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity(activity))
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var time = clock.GetCurrentInstant();

                    var document = await schedulerStore.DequeueAsync(time, ct);
                    // Also reset dead entries regularly when we are busy, otherwise they would never be handled.
                    if (document == null || time - lastReset > Duration.FromTimeSpan(schedulerOptions.FailedTimeout))
                    {
                        var oldTime = time.PlusTicks(-schedulerOptions.FailedTimeout.Ticks);

                        await schedulerStore.ResetDeadAsync(oldTime, time, ct);

                        lastReset = time;
                    }

                    // If nothing has been queried we end our loop, if somethine has been returned it is very likely there is something else.
                    if (document == null)
                    {
                        break;
                    }

                    if (!await actionBlock.SendAsync(document, ct))
                    {
                        // The block does not accept jobs anymore, so the job is queued again immediately.
                        await schedulerStore.RetryAsync(document.Id, time, default);
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Failed to dequeue job.");
                    break;
                }
            }
        }
    }
}
