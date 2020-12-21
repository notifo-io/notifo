// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Notifo.Infrastructure.MongoDb.Scheduling
{
    [Trait("Category", "Dependencies")]
    public class MongoDbSchedulerStoreTests : IClassFixture<MongoDbSchedulerStoreFixture>
    {
        private readonly Instant now = SystemClock.Instance.GetCurrentInstant();

        public MongoDbSchedulerStoreFixture _ { get; }

        public MongoDbSchedulerStoreTests(MongoDbSchedulerStoreFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_schedule_With_due_time()
        {
            var time = now.Plus(Duration.FromSeconds(1000));

            await _.Store.EnqueueScheduledAsync("1", 1, time);

            var notDequeued = await _.Store.DequeueAsync(now);

            Assert.Null(notDequeued);

            var dequeued = await _.Store.DequeueAsync(time);

            Assert.NotNull(dequeued);

            var dequeuedAgain = await _.Store.DequeueAsync(time);

            Assert.Null(dequeuedAgain);
        }

        [Fact]
        public async Task Should_schedule_with_delay()
        {
            var delay1 = Duration.FromSeconds(60 * 1000);
            var delay2 = Duration.FromSeconds(120 * 1000);

            await _.Store.EnqueueWithDelayAsync("2", 3, now.Plus(delay1));
            await _.Store.EnqueueWithDelayAsync("2", 4, now.Plus(delay2));

            var notDequeued = await _.Store.DequeueAsync(now);

            Assert.Null(notDequeued);

            var dequeued = await _.Store.DequeueAsync(now.Plus(delay2));

            Assert.Equal(new List<int> { 3, 4 }, dequeued!.Jobs);
        }
    }
}
