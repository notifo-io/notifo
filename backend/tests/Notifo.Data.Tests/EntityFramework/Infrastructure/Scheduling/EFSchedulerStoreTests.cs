// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Notifo.EntityFramework.TestHelpers;
using Notifo.Infrastructure.Scheduling;
using Notifo.Infrastructure.Scheduling.Implementation.TimerBased;
using Notifo.Shared;

namespace Notifo.EntityFramework.Infrastructure.Scheduling;

public abstract class EFSchedulerStoreTests<TContext>(ISqlFixture<TContext> fixture) : SchedulerStoreTests where TContext : DbContext
{
    protected override Task<ISchedulerStore<int>> CreateSutAsync()
    {
        var sut = new EFSchedulerStore<TContext, int>(fixture.DbContextFactory, TestJson.Options, new SchedulerOptions { QueueName = Guid.NewGuid().ToString() });

        return Task.FromResult<ISchedulerStore<int>>(sut);
    }
}
