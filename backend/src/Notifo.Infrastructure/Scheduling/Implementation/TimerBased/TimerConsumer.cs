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

namespace Notifo.Infrastructure.Scheduling.Implementation.TimerBased
{
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
                    try
                    {
                        await HandleAsync(batch);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new AggregateException(ex);
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
            actionBlock.Complete();

            if (timer != null)
            {
                await timer.StopAsync();
            }

            await actionBlock.Completion;
        }

        public async Task ExecuteInlineAsync(string key, T job)
        {
            try
            {
                await onSuccess(new List<T> { job }, false, default);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to handle job.");

                var nextTime = clock.GetCurrentInstant().Plus(Duration.FromMinutes(5));

                await schedulerStore.EnqueueScheduledAsync(key, job, nextTime, 1);
            }
        }

        private async Task HandleAsync(SchedulerBatch<T> document)
        {
            if (document.RetryCount > schedulerOptions.ExecutionRetries.Length)
            {
                await schedulerStore.CompleteAsync(document.Id);
                return;
            }

            var canRetry = document.RetryCount < schedulerOptions.ExecutionRetries.Length;

            try
            {
                var isConfirmed = true;

                if (Debugger.IsAttached)
                {
                    isConfirmed = await onSuccess(document.Jobs, !canRetry, default);
                }
                else
                {
                    using (var timeout = new CancellationTokenSource(schedulerOptions.Timeout))
                    {
                        isConfirmed = await onSuccess(document.Jobs, !canRetry, timeout.Token);
                    }
                }

                if (isConfirmed)
                {
                    await schedulerStore.CompleteAsync(document.Id);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to handle job.");

                if (canRetry)
                {
                    var wait = Duration.FromMilliseconds(schedulerOptions.ExecutionRetries[document.RetryCount]);

                    var nextTime = document.DueTime.Plus(wait);

                    await schedulerStore.RetryAsync(document.Id, nextTime);
                }
                else
                {
                    try
                    {
                        await onError(document.Jobs, ex, default);
                    }
                    catch (Exception ex2)
                    {
                        log.LogError(ex2, "Failed to handle job.");
                    }
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

                        if (document == null)
                        {
                            var oldTime = time.PlusTicks(-schedulerOptions.FailedTimeout.Ticks);

                            // If we are not busy, we can reset the dead entries.
                            await schedulerStore.ResetDeadAsync(oldTime, time, ct);

                            // If nothing has been queried we end our loop, if somethine has been returned it is very likely there is something else.
                            break;
                        }

                        await actionBlock.SendAsync(document, ct);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "Failed to dequeue job.");
                    }
                }
            }
        }
    }
}
