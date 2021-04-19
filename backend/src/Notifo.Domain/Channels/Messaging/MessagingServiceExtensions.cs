// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Messaging;
using Notifo.Infrastructure.Scheduling;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MessagingServiceExtensions
    {
        public static void AddMyMessagingChannel(this IServiceCollection services)
        {
            services.AddSingletonAs<MessagingChannel>()
                .As<ICommunicationChannel>().As<IScheduleHandler<MessagingJob>>();

            services.AddScheduler<MessagingJob>(new SchedulerOptions { QueueName = Providers.Messaging, ExecutionRetries = Array.Empty<int>() });
        }
    }
}
