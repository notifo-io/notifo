// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Notifo.Infrastructure.Scheduling
{
    public interface ISchedulerProvider
    {
        IScheduler<T> GetScheduler<T>(IServiceProvider serviceProvider, SchedulerOptions options);
    }
}
