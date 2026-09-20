// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.Scheduling;
using Notifo.Infrastructure.Scheduling.Implementation.TimerBased;
using Notifo.MongoDb.TestHelpers;
using Notifo.Shared;

namespace Notifo.MongoDb.Infrastructure.Scheduling;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoDbSchedulerStoreTests(MongoFixture fixture) : SchedulerStoreTests
{
    protected override async Task<ISchedulerStore<int>> CreateSutAsync()
    {
        var sut = new MongoDbSchedulerStore<int>(fixture.Database, new SchedulerOptions { QueueName = Guid.NewGuid().ToString() });

        await sut.InitializeAsync(default);
        return sut;
    }
}
