// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Infrastructure.Scheduling.Implementation.TimerBased;

public sealed class EFSchedulingProvider<TContext> : ISchedulingProvider where TContext : DbContext
{
    public IScheduling<T> GetScheduling<T>(IServiceProvider serviceProvider, SchedulerOptions options)
    {
        var schedulerStore = ActivatorUtilities.CreateInstance<EFSchedulerStore<TContext, T>>(serviceProvider, options);
        var scheduler = ActivatorUtilities.CreateInstance<TimerScheduling<T>>(serviceProvider, options, schedulerStore);

        return scheduler;
    }
}
