// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.Scheduling;
using Notifo.Infrastructure.Scheduling.Implementation;
using static Microsoft.Extensions.DependencyInjection.DependencyInjectionExtensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SchedulerServiceExtensions
    {
        public static void AddScheduler<T>(this IServiceCollection services, string name, SchedulerOptions? options = null)
        {
            options ??= new SchedulerOptions();
            options.QueueName = name;

            services.AddSingletonAs(c => c.GetRequiredService<ISchedulingProvider>().GetScheduling<T>(c, options));

            services.AddSingletonAs<DelegatingScheduler<T>>()
                .As<IScheduler<T>>();

            services.AddSingletonAs<DelegatingScheduleHandler<T>>()
                .AsSelf();
        }

        public static InterfaceRegistrator<T> AsSchedulingHandler<T>(this InterfaceRegistrator<T> registrator) where T : IScheduleHandler<T>
        {
            return registrator.As<IScheduleHandler<T>>();
        }
    }
}
