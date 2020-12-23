// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NodaTime;
using Notifo.Infrastructure.Initialization;
using Notifo.Infrastructure.Timers;
using Squidex.Log;

namespace Notifo.Infrastructure.Scheduling.TimerBased
{
    public sealed class TimerScheduler<T> : IScheduler<T>, IInitializable
    {
        private readonly List<CompletionTimer> timers = new List<CompletionTimer>();
        private readonly ISchedulerStore<T> schedulerStore;
        private readonly SchedulerOptions schedulerOptions;
        private readonly ActionBlock<SchedulerBatch<T>> actionBlock;
        private readonly IClock clock;
        private readonly ISemanticLog log;
        private IScheduleHandler<T>? currentHandler;

        private Instant Now
        {
            get { return clock.GetCurrentInstant(); }
        }

        public TimerScheduler(ISchedulerStore<T> schedulerStore, SchedulerOptions schedulerOptions,
            IClock clock, ISemanticLog log)
        {
            this.schedulerStore = schedulerStore;
            this.schedulerOptions = schedulerOptions;

            actionBlock = new ActionBlock<SchedulerBatch<T>>(HandleAsync, new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = schedulerOptions.MaxParallelism * 4,
                MaxDegreeOfParallelism = schedulerOptions.MaxParallelism,
                MaxMessagesPerTask = 1
            });

            this.clock = clock;

            this.log = log;
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            await schedulerStore.InitializeAsync(ct);

            timers.Add(new CompletionTimer(500, QueryAsync));
        }

        public async Task ReleaseAsync(CancellationToken ct = default)
        {
            actionBlock.Complete();

            await Task.WhenAll(timers.Select(x => x.StopAsync()));
            await actionBlock.Completion;

            await schedulerStore.ReleaseAsync(ct);
        }

        public void Subscribe(IScheduleHandler<T> handler)
        {
            if (currentHandler == null)
            {
                currentHandler = handler;
            }
        }

        public async Task ScheduleAsync(string key, T job, Instant dueTime, bool canInline, CancellationToken ct = default)
        {
            if (dueTime <= Now && canInline && schedulerOptions.ExecuteInline)
            {
                await ExecuteInlineAsync(key, job);
            }
            else
            {
                await schedulerStore.EnqueueScheduledAsync(key, job, dueTime, 0, ct);
            }
        }

        public async Task ScheduleDelayedAsync(string key, T job, int delaySeconds, bool canInline, CancellationToken ct = default)
        {
            if (delaySeconds <= 0 && canInline)
            {
                await ExecuteInlineAsync(key, job);
            }
            else
            {
                var delayTime = clock.GetCurrentInstant().Plus(Duration.FromSeconds(delaySeconds));

                await schedulerStore.EnqueueWithDelayAsync(key, job, delayTime, 0, ct);
            }
        }

        private async Task ExecuteInlineAsync(string key, T job)
        {
            try
            {
                if (currentHandler != null)
                {
                    await currentHandler.HandleAsync(new List<T> { job }, false, default);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "HandleJob")
                    .WriteProperty("status", "Failed"));

                var nextTime = Now.Plus(Duration.FromMinutes(5));

                await schedulerStore.EnqueueScheduledAsync(key, job, nextTime, 1);
            }
        }

        private async Task HandleAsync(SchedulerBatch<T> document)
        {
            var canRetry = document.RetryCount < schedulerOptions.ExecutionRetries.Length;

            try
            {
                if (currentHandler != null)
                {
                    using (var timeout = new CancellationTokenSource(schedulerOptions.Timeout))
                    {
                        await currentHandler.HandleAsync(document.Jobs, !canRetry, timeout.Token);
                    }
                }

                await schedulerStore.CompleteAsync(document.Id);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "HandleJob")
                    .WriteProperty("status", "Failed"));

                if (canRetry)
                {
                    var wait = Duration.FromMilliseconds(schedulerOptions.ExecutionRetries[document.RetryCount]);

                    var nextTime = document.DueTime.Plus(wait);

                    await schedulerStore.RetryAsync(document.Id, nextTime);
                }
                else if (currentHandler != null)
                {
                    try
                    {
                        await currentHandler.HandleExceptionAsync(document.Jobs, ex);
                    }
                    catch (Exception ex2)
                    {
                        log.LogFatal(ex2, w => w
                            .WriteProperty("action", "HandleJobException")
                            .WriteProperty("status", "Failed"));
                    }
                }
            }
        }

        public async Task QueryAsync(CancellationToken ct)
        {
            if (currentHandler == null)
            {
                return;
            }

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var time = clock.GetCurrentInstant();

                    var document = await schedulerStore.DequeueAsync(time);

                    if (document == null)
                    {
                        var oldTime = time.PlusTicks(-schedulerOptions.FailedTimeout.Ticks);

                        await schedulerStore.ResetDeadAsync(oldTime, time);
                        break;
                    }

                    await actionBlock.SendAsync(document, ct);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, w => w
                        .WriteProperty("action", "DequeueJobs")
                        .WriteProperty("status", "Failed"));
                }
            }
        }
    }
}
