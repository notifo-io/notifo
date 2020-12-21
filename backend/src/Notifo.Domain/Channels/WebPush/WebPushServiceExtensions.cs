// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.WebPush;
using Notifo.Infrastructure.Scheduling;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WebPushServiceExtensions
    {
        public static void AddMyWebPushChannel(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<WebPushOptions>(
                config.GetSection(Providers.WebPush));

            services.AddSingletonAs<WebPushChannel>()
                .As<ICommunicationChannel>().As<IScheduleHandler<WebPushJob>>().As<IWebPushService>();

            services.AddScheduler<WebPushJob>(new SchedulerOptions { QueueName = Providers.WebPush });
        }
    }
}
