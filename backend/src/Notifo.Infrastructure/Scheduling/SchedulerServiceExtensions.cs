// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.Scheduling;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SchedulerServiceExtensions
    {
        public static void AddMyScheduling(this IServiceCollection services)
        {
            services.AddMongoScheduler();
        }

        public static void AddScheduler<T>(this IServiceCollection services, SchedulerOptions options)
        {
            services.AddSingletonAs(c => c.GetRequiredService<ISchedulerProvider>().GetScheduler<T>(c, options))
                .As<IScheduler<T>>();
        }
    }
}
