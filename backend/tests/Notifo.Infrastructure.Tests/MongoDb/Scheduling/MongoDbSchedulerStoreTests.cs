// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Infrastructure.Fixtures;
using Notifo.Infrastructure.Scheduling;
using Notifo.Infrastructure.Scheduling.Implementation.TimerBased.MongoDb;

namespace Notifo.Infrastructure.MongoDb.Scheduling;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoDbSchedulerStoreTests(MongoFixture fixture) : IAsyncLifetime
{
    private readonly Instant now = SystemClock.Instance.GetCurrentInstant();
    private readonly MongoDbSchedulerStore<int> store =
        new MongoDbSchedulerStore<int>(fixture.Database, new SchedulerOptions { QueueName = Guid.NewGuid().ToString() });

    public Task InitializeAsync()
    {
        return store.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Should_schedule_with_due_time()
    {
        var time = now.Plus(Duration.FromSeconds(1000));

        await store.EnqueueAsync("1", 1, time, 0, default);

        var notDequeued = await store.DequeueAsync(now, default);

        Assert.Null(notDequeued);

        var dequeued = await store.DequeueAsync(time, default);

        Assert.NotNull(dequeued);
        Assert.Equal([1], dequeued!.GetAllJobs());

        var dequeuedAgain = await store.DequeueAsync(time, default);

        Assert.Null(dequeuedAgain);
    }

    [Fact]
    public async Task Should_schedule_grouped_with_delay()
    {
        var delay1 = Duration.FromSeconds(60 * 1000);
        var delay2 = Duration.FromSeconds(120 * 1000);

        await store.EnqueueGroupedAsync("1", "group-a", 1, now.Plus(delay1), 0, default);
        await store.EnqueueGroupedAsync("2", "group-a", 2, now.Plus(delay2), 0, default);

        var notDequeued = await store.DequeueAsync(now, default);

        Assert.Null(notDequeued);

        var dequeued = await store.DequeueAsync(now.Plus(delay2), default);

        Assert.NotNull(dequeued);
        Assert.Equal([1, 2], dequeued!.GetAllJobs());
    }

    [Fact]
    public async Task Should_schedule_grouped_with_delay_and_eliminate_duplicates()
    {
        var delay1 = Duration.FromSeconds(60 * 1000);
        var delay2 = Duration.FromSeconds(120 * 1000);

        await store.EnqueueGroupedAsync("1", "2", 3, now.Plus(delay1), 0, default);
        await store.EnqueueGroupedAsync("1", "2", 4, now.Plus(delay2), 0, default);

        var notDequeued = await store.DequeueAsync(now, default);

        Assert.Null(notDequeued);

        var dequeued = await store.DequeueAsync(now.Plus(delay2), default);

        Assert.NotNull(dequeued);
        Assert.Equal([4], dequeued!.GetAllJobs());
    }

    [Fact]
    public async Task Should_remove_key_from_group()
    {
        var delay1 = Duration.FromSeconds(60 * 1000);
        var delay2 = Duration.FromSeconds(120 * 1000);

        await store.EnqueueGroupedAsync("1", "group-a", 1, now.Plus(delay1), 0, default);
        await store.EnqueueGroupedAsync("2", "group-a", 2, now.Plus(delay2), 0, default);

        await store.CompleteByKeyAsync("1", "group-a", default);

        var dequeued = await store.DequeueAsync(now.Plus(delay2), default);

        Assert.NotNull(dequeued);
        Assert.Equal([2], dequeued!.GetAllJobs());
    }

    [Fact]
    public async Task Should_remove_multiple_keys_from_group()
    {
        var delay1 = Duration.FromSeconds(60 * 1000);
        var delay2 = Duration.FromSeconds(120 * 1000);

        await store.EnqueueGroupedAsync("1", "group-a", 1, now.Plus(delay1), 0, default);
        await store.EnqueueGroupedAsync("1", "group-a", 1, now.Plus(delay2), 0, default);

        await store.CompleteByKeyAsync("1", "group-a", default);
        await store.CompleteByKeyAsync("1", "group-a", default);

        var dequeued = await store.DequeueAsync(now.Plus(delay2), default);

        Assert.Null(dequeued);
    }

    [Fact]
    public async Task Should_schedule_job_with_dot_in_key()
    {
        await store.EnqueueAsync("app.id_user.id", 42, now, 0, default);

        var dequeued = await store.DequeueAsync(now, default);

        Assert.NotNull(dequeued);
        Assert.Equal([42], dequeued!.GetAllJobs());
    }

    [Fact]
    public async Task Should_remove_job_with_dot_in_key_from_group()
    {
        await store.EnqueueGroupedAsync("app.id_1", "group-a", 1, now, 0, default);
        await store.EnqueueGroupedAsync("app.id_2", "group-a", 2, now, 0, default);

        var removed = await store.CompleteByKeyAsync("app.id_1", "group-a", default);

        var dequeued = await store.DequeueAsync(now, default);

        Assert.True(removed);
        Assert.NotNull(dequeued);
        Assert.Equal([2], dequeued!.GetAllJobs());
    }

    [Fact]
    public async Task Should_not_handle_grouped_batch_before_due_time_of_last_job()
    {
        var delay = Duration.FromSeconds(60 * 1000);

        await store.EnqueueGroupedAsync("1", "group-a", 1, now, 0, default);
        await store.EnqueueGroupedAsync("2", "group-a", 2, now.Plus(delay), 0, default);

        var notDequeued = await store.DequeueAsync(now, default);

        var dequeued = await store.DequeueAsync(now.Plus(delay), default);

        Assert.Null(notDequeued);
        Assert.NotNull(dequeued);
        Assert.Equal([1, 2], dequeued!.GetAllJobs());
    }

    [Fact]
    public async Task Should_update_due_time_if_job_is_scheduled_again_with_earlier_time()
    {
        var delay = Duration.FromSeconds(60 * 1000);

        await store.EnqueueAsync("1", 1, now.Plus(delay), 0, default);
        await store.EnqueueAsync("1", 2, now, 0, default);

        var dequeued = await store.DequeueAsync(now, default);

        Assert.NotNull(dequeued);
        Assert.Equal([2], dequeued!.GetAllJobs());
    }

    [Fact]
    public async Task Should_not_remove_key_from_progressing_group()
    {
        await store.EnqueueGroupedAsync("1", "group-a", 1, now, 0, default);

        var dequeued = await store.DequeueAsync(now, default);

        var removed = await store.CompleteByKeyAsync("1", "group-a", default);

        Assert.NotNull(dequeued);
        Assert.False(removed);
    }

    [Fact]
    public async Task Should_remove_key_from_pending_group_if_other_group_is_progressing()
    {
        var delay = Duration.FromSeconds(60 * 1000);

        await store.EnqueueGroupedAsync("1", "group-a", 1, now, 0, default);

        var dequeued1 = await store.DequeueAsync(now, default);

        await store.EnqueueGroupedAsync("2", "group-a", 2, now.Plus(delay), 0, default);

        var removed = await store.CompleteByKeyAsync("2", "group-a", default);

        var dequeued2 = await store.DequeueAsync(now.Plus(delay), default);

        Assert.NotNull(dequeued1);
        Assert.True(removed);
        Assert.Null(dequeued2);
    }

    [Fact]
    public async Task Should_not_remove_group_if_other_key_exists()
    {
        await store.EnqueueGroupedAsync("1", "group-a", 1, now, 0, default);
        await store.EnqueueGroupedAsync("2", "group-a", 2, now, 0, default);

        var removed = await store.CompleteByKeyAsync("1", "group-a", default);

        var dequeued = await store.DequeueAsync(now, default);

        Assert.True(removed);
        Assert.NotNull(dequeued);
        Assert.Equal([2], dequeued!.GetAllJobs());
    }
}
