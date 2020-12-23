// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Infrastructure.Scheduling.TimerBased.MongoDb
{
    public sealed class MongoDbSchedulerProvider : ISchedulerProvider
    {
        public IScheduler<T> GetScheduler<T>(IServiceProvider serviceProvider, SchedulerOptions options)
        {
            var schedulerStore = ActivatorUtilities.CreateInstance<MongoDbSchedulerStore<T>>(serviceProvider, options);
            var scheduler = ActivatorUtilities.CreateInstance<TimerScheduler<T>>(serviceProvider, options, schedulerStore);

            return scheduler;
        }
    }
}
