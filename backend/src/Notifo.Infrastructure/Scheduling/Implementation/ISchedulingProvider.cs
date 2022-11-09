// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Scheduling.Implementation;

public interface ISchedulingProvider
{
    IScheduling<T> GetScheduling<T>(IServiceProvider serviceProvider, SchedulerOptions options);
}
