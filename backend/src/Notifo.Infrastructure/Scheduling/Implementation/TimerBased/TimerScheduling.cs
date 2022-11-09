// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Notifo.Infrastructure.Tasks;
using Squidex.Hosting;

namespace Notifo.Infrastructure.Scheduling.Implementation.TimerBased;

public sealed class TimerScheduling<T> : IScheduling<T>
{
    private readonly ISchedulerStore<T> schedulerStore;
    private readonly SchedulerOptions schedulerOptions;
    private readonly IClock clock;
    private readonly ILogger<TimerScheduling<T>> log;
    private TimerConsumer<T>? consumer;

    public int Order => 1000;

    public string Name => $"TimerScheduler({schedulerOptions.QueueName})";

    public TimerScheduling(ISchedulerStore<T> schedulerStore, SchedulerOptions schedulerOptions,
        ILogger<TimerScheduling<T>> log, IClock clock)
    {
        this.schedulerStore = schedulerStore;
        this.schedulerOptions = schedulerOptions;
        this.clock = clock;

        this.log = log;
    }

    public async Task InitializeAsync(
        CancellationToken ct)
    {
        if (schedulerStore is IInitializable initializable)
        {
            await initializable.InitializeAsync(ct);
        }
    }

    public async Task ReleaseAsync(
        CancellationToken ct)
    {
        if (schedulerStore is IInitializable initializable)
        {
            await initializable.ReleaseAsync(ct);
        }

        if (consumer != null)
        {
            await consumer.StopAsync();
        }
    }

    public Task ScheduleAsync(string key, T job, Duration dueTimeFromNow, bool canInline,
        CancellationToken ct = default)
    {
        var now = clock.GetCurrentInstant();

        return ScheduleAsync(key, job, now.Plus(dueTimeFromNow), canInline, ct);
    }

    public Task ScheduleGroupedAsync(string key, T job, Duration dueTimeFromNow, bool canInline,
        CancellationToken ct = default)
    {
        var now = clock.GetCurrentInstant();

        return ScheduleAsync(key, job, now.Plus(dueTimeFromNow), canInline, ct);
    }

    public async Task ScheduleAsync(string key, T job, Instant dueTime, bool canInline,
        CancellationToken ct = default)
    {
        var now = clock.GetCurrentInstant();

        if (dueTime <= now && canInline && schedulerOptions.ExecuteInline)
        {
            if (consumer != null)
            {
                await consumer.ExecuteInlineAsync(key, job);
            }
        }
        else
        {
            await schedulerStore.EnqueueScheduledAsync(key, job, dueTime, 0, ct);
        }
    }

    public async Task ScheduleGroupedAsync(string key, T job, Instant dueTime, bool canInline,
        CancellationToken ct = default)
    {
        var now = clock.GetCurrentInstant();

        if (dueTime <= now && canInline && schedulerOptions.ExecuteInline)
        {
            if (consumer != null)
            {
                await consumer.ExecuteInlineAsync(key, job);
            }
        }
        else
        {
            await schedulerStore.EnqueueGroupedAsync(key, job, dueTime, 0, ct);
        }
    }

    public void Complete(string key)
    {
        schedulerStore.CompleteByKeyAsync(key).Forget();
    }

    public Task SubscribeAsync(ScheduleSuccessCallback<T> onSuccess, ScheduleErrorCallback<T> onError,
        CancellationToken ct = default)
    {
        consumer = new TimerConsumer<T>(schedulerStore, schedulerOptions, onSuccess, onError, log, clock);
        consumer.Subscribe();

        return Task.CompletedTask;
    }
}
