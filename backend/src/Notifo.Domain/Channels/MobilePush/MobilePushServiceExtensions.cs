// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels;
using Notifo.Domain.Channels.MobilePush;
using Notifo.Infrastructure.Scheduling;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MobilePushServiceExtensions
    {
        public static void AddMyMobilePushChannel(this IServiceCollection services)
        {
            services.AddSingletonAs<MobilePushChannel>()
                .As<ICommunicationChannel>().As<IScheduleHandler<MobilePushJob>>();

            services.AddScheduler<MobilePushJob>(new SchedulerOptions { QueueName = Providers.MobilePush });
        }
    }
}
