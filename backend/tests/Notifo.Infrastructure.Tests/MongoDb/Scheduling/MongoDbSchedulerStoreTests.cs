﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Notifo.Infrastructure.MongoDb.Scheduling;

[Trait("Category", "Dependencies")]
public class MongoDbSchedulerStoreTests(MongoDbSchedulerStoreFixture fixture) : IClassFixture<MongoDbSchedulerStoreFixture>
{
    private readonly Instant now = SystemClock.Instance.GetCurrentInstant();

    public MongoDbSchedulerStoreFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_schedule_with_due_time()
    {
        var time = now.Plus(Duration.FromSeconds(1000));

        await _.Store.EnqueueAsync("1", 1, time, 0, default);

        var notDequeued = await _.Store.DequeueAsync(now, default);

        Assert.Null(notDequeued);

        var dequeued = await _.Store.DequeueAsync(time, default);

        Assert.NotNull(dequeued);
        Assert.Equal([1], dequeued!.GetAllJobs());

        var dequeuedAgain = await _.Store.DequeueAsync(time, default);

        Assert.Null(dequeuedAgain);
    }

    [Fact]
    public async Task Should_schedule_grouped_with_delay()
    {
        var delay1 = Duration.FromSeconds(60 * 1000);
        var delay2 = Duration.FromSeconds(120 * 1000);

        await _.Store.EnqueueGroupedAsync("1", "group-a", 1, now.Plus(delay1), 0, default);
        await _.Store.EnqueueGroupedAsync("2", "group-a", 2, now.Plus(delay2), 0, default);

        var notDequeued = await _.Store.DequeueAsync(now, default);

        Assert.Null(notDequeued);

        var dequeued = await _.Store.DequeueAsync(now.Plus(delay2), default);

        Assert.NotNull(dequeued);
        Assert.Equal([1, 2], dequeued!.GetAllJobs());
    }

    [Fact]
    public async Task Should_schedule_grouped_with_delay_and_eliminate_duplicates()
    {
        var delay1 = Duration.FromSeconds(60 * 1000);
        var delay2 = Duration.FromSeconds(120 * 1000);

        await _.Store.EnqueueGroupedAsync("1", "2", 3, now.Plus(delay1), 0, default);
        await _.Store.EnqueueGroupedAsync("1", "2", 4, now.Plus(delay2), 0, default);

        var notDequeued = await _.Store.DequeueAsync(now, default);

        Assert.Null(notDequeued);

        var dequeued = await _.Store.DequeueAsync(now.Plus(delay2), default);

        Assert.NotNull(dequeued);
        Assert.Equal([4], dequeued!.GetAllJobs());
    }

    [Fact]
    public async Task Should_remove_key_from_group()
    {
        var delay1 = Duration.FromSeconds(60 * 1000);
        var delay2 = Duration.FromSeconds(120 * 1000);

        await _.Store.EnqueueGroupedAsync("1", "group-a", 1, now.Plus(delay1), 0, default);
        await _.Store.EnqueueGroupedAsync("2", "group-a", 2, now.Plus(delay2), 0, default);

        await _.Store.CompleteByKeyAsync("1", "group-a", default);

        var dequeued = await _.Store.DequeueAsync(now.Plus(delay2), default);

        Assert.NotNull(dequeued);
        Assert.Equal([2], dequeued!.GetAllJobs());
    }

    [Fact]
    public async Task Should_remove_multiple_keys_from_group()
    {
        var delay1 = Duration.FromSeconds(60 * 1000);
        var delay2 = Duration.FromSeconds(120 * 1000);

        await _.Store.EnqueueGroupedAsync("1", "group-a", 1, now.Plus(delay1), 0, default);
        await _.Store.EnqueueGroupedAsync("1", "group-a", 1, now.Plus(delay2), 0, default);

        await _.Store.CompleteByKeyAsync("1", "group-a", default);
        await _.Store.CompleteByKeyAsync("1", "group-a", default);

        var dequeued = await _.Store.DequeueAsync(now.Plus(delay2), default);

        Assert.Null(dequeued);
    }
}
