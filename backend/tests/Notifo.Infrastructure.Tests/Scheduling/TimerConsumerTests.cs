// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Notifo.Infrastructure.Scheduling.Implementation;
using Notifo.Infrastructure.Scheduling.Implementation.TimerBased;

namespace Notifo.Infrastructure.Scheduling;

public class TimerConsumerTests
{
    private readonly ISchedulerStore<int> store = A.Fake<ISchedulerStore<int>>();
    private readonly ScheduleSuccessCallback<int> onSuccess = A.Fake<ScheduleSuccessCallback<int>>();
    private readonly ScheduleErrorCallback<int> onError = A.Fake<ScheduleErrorCallback<int>>();
    private readonly IClock clock = A.Fake<IClock>();
    private readonly Instant now = Instant.FromUtc(2020, 1, 1, 12, 0, 0);
    private readonly SchedulerOptions options = new SchedulerOptions
    {
        ExecutionRetries = [5000, 10000],
        QueueName = "Test"
    };

    public TimerConsumerTests()
    {
        A.CallTo(() => clock.GetCurrentInstant())
            .Returns(now);

        A.CallTo(() => onSuccess(A<List<int>>._, A<bool>._, A<CancellationToken>._))
            .Returns(true);
    }

    [Fact]
    public async Task Should_complete_batch_if_handled()
    {
        var batch = CreateBatch(retryCount: 0);

        await HandleAsync(batch);

        A.CallTo(() => onSuccess(A<List<int>>.That.IsSameSequenceAs(new[] { 1 }), false, A<CancellationToken>._))
            .MustHaveHappened();

        A.CallTo(() => store.CompleteAsync(batch.Id, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_retry_based_on_current_time_if_handling_failed()
    {
        var batch = CreateBatch(retryCount: 1);

        A.CallTo(() => onSuccess(A<List<int>>._, A<bool>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException());

        await HandleAsync(batch);

        A.CallTo(() => store.RetryAsync(batch.Id, now.Plus(Duration.FromMilliseconds(10000)), A<CancellationToken>._))
            .MustHaveHappened();

        A.CallTo(() => onError(A<List<int>>._, A<Exception>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_call_error_and_complete_if_last_attempt_failed()
    {
        var batch = CreateBatch(retryCount: 2);

        A.CallTo(() => onSuccess(A<List<int>>._, true, A<CancellationToken>._))
            .Throws(new InvalidOperationException());

        await HandleAsync(batch);

        A.CallTo(() => onError(A<List<int>>._, A<InvalidOperationException>._, A<CancellationToken>._))
            .MustHaveHappened();

        A.CallTo(() => store.CompleteAsync(batch.Id, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_call_error_and_complete_if_retries_exceeded()
    {
        var batch = CreateBatch(retryCount: 3);

        await HandleAsync(batch);

        A.CallTo(() => onSuccess(A<List<int>>._, A<bool>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => onError(A<List<int>>._, A<Exception>._, A<CancellationToken>._))
            .MustHaveHappened();

        A.CallTo(() => store.CompleteAsync(batch.Id, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_handle_next_batch_if_store_failed()
    {
        var batch1 = CreateBatch(retryCount: 0);
        var batch2 = CreateBatch(retryCount: 0);

        A.CallTo(() => store.CompleteAsync(batch1.Id, A<CancellationToken>._))
            .Throws(new InvalidOperationException());

        await HandleAsync(batch1, batch2);

        A.CallTo(() => store.CompleteAsync(batch2.Id, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_retry_inline_job_with_group_key_if_failed()
    {
        A.CallTo(() => onSuccess(A<List<int>>._, false, A<CancellationToken>._))
            .Throws(new InvalidOperationException());

        var sut = CreateSut();

        await sut.ExecuteInlineAsync("key", "group", 1);

        A.CallTo(() => store.EnqueueGroupedAsync("key", "group", 1, now.Plus(Duration.FromMilliseconds(5000)), 1, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_call_error_if_inline_job_failed_without_retries()
    {
        options.ExecutionRetries = [];

        A.CallTo(() => onSuccess(A<List<int>>._, true, A<CancellationToken>._))
            .Throws(new InvalidOperationException());

        var sut = CreateSut();

        await sut.ExecuteInlineAsync("key", null, 1);

        A.CallTo(() => onError(A<List<int>>._, A<InvalidOperationException>._, A<CancellationToken>._))
            .MustHaveHappened();

        A.CallTo(() => store.EnqueueAsync(A<string>._, A<int>._, A<Instant>._, A<int>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_enqueue_job_if_no_consumer_subscribed()
    {
        var sut = new TimerScheduling<int>(store, options, A.Fake<ILogger<TimerScheduling<int>>>(), clock);

        await sut.ScheduleAsync("key", 1, now, true);

        A.CallTo(() => store.EnqueueAsync("key", 1, now, 0, A<CancellationToken>._))
            .MustHaveHappened();
    }

    private async Task HandleAsync(params SchedulerBatch<int>[] batches)
    {
        A.CallTo(() => store.DequeueAsync(A<Instant>._, A<CancellationToken>._))
            .ReturnsNextFromSequence([.. batches, null]);

        var sut = CreateSut();

        await sut.QueryAsync(default);
        await sut.StopAsync();
    }

    private TimerConsumer<int> CreateSut()
    {
        return new TimerConsumer<int>(store, options, onSuccess, onError, A.Fake<ILogger>(), clock);
    }

    private SchedulerBatch<int> CreateBatch(int retryCount)
    {
        return new SchedulerBatch<int>
        {
            Id = Guid.NewGuid().ToString(),
            DueTime = now.Minus(Duration.FromHours(1)),
            JobsV2 = new Dictionary<string, int>
            {
                ["1"] = 1
            },
            RetryCount = retryCount
        };
    }
}
