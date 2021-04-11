// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Sms;
using Notifo.Infrastructure.Scheduling;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SmsServiceExtensions
    {
        public static void AddMySmsChannel(this IServiceCollection services)
        {
            services.AddSingletonAs<SmsChannel>()
                .As<ICommunicationChannel>().As<IScheduleHandler<SmsJob>>();

            services.AddScheduler<SmsJob>(new SchedulerOptions { QueueName = Providers.Sms, ExecutionRetries = Array.Empty<int>() });
        }
    }
}
