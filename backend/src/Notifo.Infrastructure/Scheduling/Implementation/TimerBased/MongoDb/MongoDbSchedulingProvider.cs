// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Infrastructure.Scheduling.Implementation.TimerBased.MongoDb
{
    public sealed class MongoDbSchedulingProvider : ISchedulingProvider
    {
        public IScheduling<T> GetScheduling<T>(IServiceProvider serviceProvider, SchedulerOptions options)
        {
            var schedulerStore = ActivatorUtilities.CreateInstance<MongoDbSchedulerStore<T>>(serviceProvider, options);
            var scheduler = ActivatorUtilities.CreateInstance<TimerScheduling<T>>(serviceProvider, options, schedulerStore);

            return scheduler;
        }
    }
}
