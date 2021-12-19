// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using FakeItEasy;
using Notifo.Domain.Counters;
using Squidex.Log;
using Xunit;

namespace Notifo.Domain.Counter
{
    public class CounterCollectorTests
    {
        private readonly ICounterStore<string> store = A.Fake<ICounterStore<string>>();
        private readonly CounterCollector<string> sut;

        public CounterCollectorTests()
        {
            sut = new CounterCollector<string>(store, A.Fake<ISemanticLog>(), 100, 10, 100);
        }

        [Fact]
        public async Task Should_batch_writes()
        {
            var longDelay = new CounterCollector<string>(store, A.Fake<ISemanticLog>(), 100, 10, 10000);

            for (var i = 0; i < 100; i++)
            {
                var key = i.ToString(CultureInfo.InvariantCulture);

                await longDelay.AddAsync(key, new CounterMap());
            }

            await longDelay.DisposeAsync();

            A.CallTo(() => store.BatchWriteAsync(A<List<(string, CounterMap)>>.That.Matches(x => x.Count == 10), default))
                .MustHaveHappenedANumberOfTimesMatching(x => x == 10);
        }

        [Fact]
        public async Task Should_write_after_delay()
        {
            for (var i = 0; i < 9; i++)
            {
                var key = i.ToString(CultureInfo.InvariantCulture);

                await sut.AddAsync(key, new CounterMap());
            }

            A.CallTo(() => store.BatchWriteAsync(A<List<(string, CounterMap)>>._, default))
                .MustNotHaveHappened();

            await Task.Delay(1000);

            A.CallTo(() => store.BatchWriteAsync(A<List<(string, CounterMap)>>.That.Matches(x => x.Count == 9), default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_group_after_delay()
        {
            for (var i = 0; i < 9; i++)
            {
                var key = "1";

                await sut.AddAsync(key, new CounterMap());
            }

            A.CallTo(() => store.BatchWriteAsync(A<List<(string, CounterMap)>>._, default))
                .MustNotHaveHappened();

            await Task.Delay(1000);

            A.CallTo(() => store.BatchWriteAsync(A<List<(string, CounterMap)>>.That.Matches(x => x.Count == 1), default))
                .MustHaveHappened();
        }
    }
}
